﻿using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace Agile.Abp.Identity
{
    public class ChangePhoneNumberInput
    {
        /// <summary>
        /// 新手机号
        /// </summary>
        [Required]
        [Phone]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPhoneNumberLength))]
        public string NewPhoneNumber { get; set; }
        /// <summary>
        /// 安全验证码
        /// </summary>
        [DisableAuditing]
        [StringLength(6)]
        public string Code { get; set; }
    }
}
