﻿using Microsoft.EntityFrameworkCore;

using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace BackendAdmin.EntityFrameworkCore
{
    public class BackendAdminHostMigrationsDbContext : AbpDbContext<BackendAdminHostMigrationsDbContext>
    {
        public BackendAdminHostMigrationsDbContext(DbContextOptions<BackendAdminHostMigrationsDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureBackendAdmin();

            modelBuilder.ConfigureAuditLogging();
            modelBuilder.ConfigureFeatureManagement();
            modelBuilder.ConfigureTenantManagement();
            modelBuilder.ConfigureSettingManagement();
            modelBuilder.ConfigurePermissionManagement();
        }
    }
}
