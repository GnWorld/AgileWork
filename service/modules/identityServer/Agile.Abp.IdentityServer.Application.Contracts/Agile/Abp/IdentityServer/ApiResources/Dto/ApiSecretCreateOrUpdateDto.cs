﻿namespace Agile.Abp.IdentityServer.ApiResources
{
    public class ApiSecretCreateOrUpdateDto : SecretBaseDto
    {
        public HashType HashType { get; set; }
    }
}
