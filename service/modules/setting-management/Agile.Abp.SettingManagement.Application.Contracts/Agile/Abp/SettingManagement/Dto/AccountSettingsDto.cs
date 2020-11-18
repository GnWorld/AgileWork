﻿namespace Agile.Abp.SettingManagement
{
    public class AccountSettingsDto
    {
        /// <summary>
        /// 是否允许用户自行注册帐户
        /// </summary>
        public bool IsSelfRegistrationEnabled { get; set; }
        /// <summary>
        /// 服务器是否将允许用户使用本地帐户进行身份验证
        /// </summary>
        public bool EnableLocalLogin { get; set; }
    }
}
