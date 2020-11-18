﻿using Agile.Abp.Features.LimitValidation.Localization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Agile.Abp.Features.LimitValidation
{
    [DependsOn(typeof(AbpFeaturesModule))]
    public class AbpFeaturesLimitValidationModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.OnRegistred(FeaturesLimitValidationInterceptorRegistrar.RegisterIfNeeded);

            Configure<AbpFeaturesLimitValidationOptions>(options =>
            {
                options.MapDefaultEffectPolicys();
            });

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<AbpFeaturesLimitValidationModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<FeaturesLimitValidationResource>("en")
                    .AddVirtualJson("/Agile/Abp/Features/LimitValidation/Localization/Resources");
            });
        }
    }
}
