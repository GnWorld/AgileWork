﻿using Volo.Abp.Application.Dtos;

namespace Agile.Abp.IdentityServer.IdentityResources
{
    public class IdentityResourceGetByPagedDto : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}
