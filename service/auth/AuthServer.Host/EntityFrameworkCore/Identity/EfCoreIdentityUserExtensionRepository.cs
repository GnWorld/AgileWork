﻿using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace AuthServer.Host.EntityFrameworkCore.Identity
{
    public class EfCoreIdentityUserRepository : EfCoreRepository<IdentityDbContext, IdentityUser, Guid>, Agile.Abp.Account.IIdentityUserRepository,
        ITransientDependency
    {
        public EfCoreIdentityUserRepository(
            IDbContextProvider<IdentityDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public virtual async Task<bool> PhoneNumberHasRegistedAsync(string phoneNumber)
        {
            return await DbSet.AnyAsync(x => x.PhoneNumberConfirmed && x.PhoneNumber.Equals(phoneNumber));
        }

        public virtual async Task<Guid?> GetIdByPhoneNumberAsync(string phoneNumber)
        {
            return await DbSet
                .Where(x => x.PhoneNumber.Equals(phoneNumber))
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<IdentityUser> FindByPhoneNumberAsync(string phoneNumber)
        {
            return await WithDetails()
                .Where(usr => usr.PhoneNumber.Equals(phoneNumber))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public override IQueryable<IdentityUser> WithDetails()
        {
            return DbSet
                .Include(x => x.Claims)
                .Include(x => x.Roles)
                .Include(x => x.Logins)
                .Include(x => x.Tokens);
        }
    }
}
