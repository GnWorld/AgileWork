﻿using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Agile.Abp.IdentityServer
{
    [DependsOn(
        typeof(AbpIdentityServerDomainModule),
        typeof(AbpIdentityServerApplicationContractsModule),
        typeof(AbpAutoMapperModule)
        )]
    public class AbpIdentityServerApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.Configurators.Add(ctx =>
                {
                    ctx.MapperConfiguration.AddProfile<AbpIdentityServerAutoMapperProfile>();
                });
            });
        }
    }
}
