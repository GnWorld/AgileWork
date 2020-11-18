﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.VirtualFileSystem;

namespace Agile.Abp.Features.LimitValidation.Redis
{
    [DisableConventionalRegistration]
    public class RedisRequiresLimitFeatureChecker : IRequiresLimitFeatureChecker
    {
        private const string CHECK_LUA_SCRIPT = "/Agile/Abp/Features/LimitValidation/Redis/Lua/check.lua";
        private const string PROCESS_LUA_SCRIPT = "/Agile/Abp/Features/LimitValidation/Redis/Lua/process.lua";

        public ILogger<RedisRequiresLimitFeatureChecker> Logger { protected get; set; }

        private volatile ConnectionMultiplexer _connection;
        private volatile ConfigurationOptions _redisConfig;
        private IDatabaseAsync _redis;
        private IServer _server;

        private IVirtualFileProvider _virtualFileProvider;
        private ICurrentTenant _currentTenant;
        private AbpRedisRequiresLimitFeatureOptions _options;
        private readonly string _instance;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisRequiresLimitFeatureChecker(
            ICurrentTenant currentTenant,
            IVirtualFileProvider virtualFileProvider,
            IOptions<AbpRedisRequiresLimitFeatureOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;
            _currentTenant = currentTenant;
            _virtualFileProvider = virtualFileProvider;

            _instance = _options.InstanceName ?? string.Empty;

            Logger = NullLogger<RedisRequiresLimitFeatureChecker>.Instance;
        }

        public virtual async Task<bool> CheckAsync(RequiresLimitFeatureContext context, CancellationToken cancellation = default)
        {
            await ConnectAsync(cancellation);

            var result = await EvaluateAsync(CHECK_LUA_SCRIPT, context, cancellation);
            return result + 1 <= context.Limit;
        }

        public virtual async Task ProcessAsync(RequiresLimitFeatureContext context, CancellationToken cancellation = default)
        {
            await ConnectAsync(cancellation);
            
            await EvaluateAsync(PROCESS_LUA_SCRIPT, context, cancellation);
        }

        private async Task<int> EvaluateAsync(string luaScriptFilePath, RequiresLimitFeatureContext context, CancellationToken cancellation = default)
        {
            var luaScriptFile = _virtualFileProvider.GetFileInfo(luaScriptFilePath);
            using var luaScriptFileStream = luaScriptFile.CreateReadStream();
            var fileBytes = await luaScriptFileStream.GetAllBytesAsync(cancellation);

            var luaSha1 = fileBytes.Sha1();
            if (!await _server.ScriptExistsAsync(luaSha1))
            {
                var luaScript = Encoding.UTF8.GetString(fileBytes);
                luaSha1 = await _server.ScriptLoadAsync(luaScript);
            }

            var keys = new RedisKey[1] { NormalizeKey(context) };
            var values = new RedisValue[] { context.GetEffectTicks() };
            var result = await _redis.ScriptEvaluateAsync(luaSha1, keys, values);
            if (result.Type == ResultType.Error)
            {
                throw new AbpException($"脚本执行错误:{result}");
            }
            return (int)result;
        }

        private string NormalizeKey(RequiresLimitFeatureContext context)
        {
            if (_currentTenant.IsAvailable)
            {
                return $"{_instance}t:RequiresLimitFeature;t:{_currentTenant.Id};f:{context.LimitFeature}";
            }
            return $"{_instance}c:RequiresLimitFeature;f:{context.LimitFeature}";
        }

        private void RegistenConnectionEvent(ConnectionMultiplexer connection)
        {
            if (connection != null)
            {
                connection.ConnectionFailed += OnConnectionFailed;
                connection.ConnectionRestored += OnConnectionRestored;
                connection.ErrorMessage += OnErrorMessage;
                connection.ConfigurationChanged += OnConfigurationChanged;
                connection.HashSlotMoved += OnHashSlotMoved;
                connection.InternalError += OnInternalError;
                connection.ConfigurationChangedBroadcast += OnConfigurationChangedBroadcast;
            }
        }

        private async Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (_redis != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(token);
            try
            {
                if (_redis == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _redisConfig = _options.ConfigurationOptions;
                    }
                    else
                    {
                        _redisConfig = ConfigurationOptions.Parse(_options.Configuration);
                    }
                    _redisConfig.AllowAdmin = true;
                    _redisConfig.SetDefaultPorts();
                    _connection = await ConnectionMultiplexer.ConnectAsync(_redisConfig);
                    RegistenConnectionEvent(_connection);
                    _redis = _connection.GetDatabase();
                    _server = _connection.GetServer(_redisConfig.EndPoints[0]);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void OnConfigurationChangedBroadcast(object sender, EndPointEventArgs e)
        {
            Logger.LogInformation("Redis server master/slave changes");
        }

        private void OnInternalError(object sender, InternalErrorEventArgs e)
        {
            Logger.LogError("Redis internal error, origin:{0}, connectionType:{1}",
                e.Origin, e.ConnectionType);
            Logger.LogError(e.Exception, "Redis internal error");

        }

        private void OnHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Logger.LogInformation("Redis configuration changed");
        }

        private void OnConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Logger.LogInformation("Redis configuration changed");
        }

        private void OnErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Logger.LogWarning("Redis error, message:{0}", e.Message);
        }

        private void OnConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Logger.LogWarning("Redis connection restored, failureType:{0}, connectionType:{1}",
                e.FailureType, e.ConnectionType);
        }

        private void OnConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Logger.LogError("Redis connection failed, failureType:{0}, connectionType:{1}",
                e.FailureType, e.ConnectionType);
            Logger.LogError(e.Exception, "Redis lock connection failed");
        }
    }
}
