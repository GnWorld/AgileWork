﻿using Agile.Abp.TenantManagement;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.TenantManagement;

namespace Agile.Abp.MultiTenancy.RemoteService.EventBus.Distributed
{
    public class TenantCreateEventHandler : IDistributedEventHandler<EntityCreatedEto<TenantEto>>, ITransientDependency
    {
        private readonly ITenantAppService _tenantAppService;
        private readonly IDistributedCache<TenantConfigurationCacheItem> _cache;

        public TenantCreateEventHandler(
            ITenantAppService tenantAppService,
            IDistributedCache<TenantConfigurationCacheItem> cache)
        {
            _cache = cache;
            _tenantAppService = tenantAppService;
        }

        public virtual async Task HandleEventAsync(EntityCreatedEto<TenantEto> eventData)
        {
            var tenantDto = await _tenantAppService.GetAsync(eventData.Entity.Id);
            var tenantConnectionStringsDto = await _tenantAppService.GetConnectionStringAsync(eventData.Entity.Id);
            var connectionStrings = new ConnectionStrings();
            foreach (var tenantConnectionString in tenantConnectionStringsDto.Items)
            {
                connectionStrings[tenantConnectionString.Name] = tenantConnectionString.Value;
            }
            var cacheItem = new TenantConfigurationCacheItem(tenantDto.Id, tenantDto.Name, connectionStrings);

            var cacheKey = TenantConfigurationCacheItem.CalculateCacheKey(eventData.Entity.Id.ToString());
            await _cache.SetAsync(cacheKey, cacheItem);
        }
    }
}
