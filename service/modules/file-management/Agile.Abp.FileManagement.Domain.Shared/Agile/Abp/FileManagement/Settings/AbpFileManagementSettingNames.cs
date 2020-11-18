﻿namespace Agile.Abp.FileManagement.Settings
{
    public class AbpFileManagementSettingNames
    {
        public const string GroupName = "Abp.FileManagement";
        /// <summary>
        /// 文件限制长度
        /// </summary>
        public const string FileLimitLength = GroupName + ".FileLimitLength";
        /// <summary>
        /// 允许的文件扩展名类型
        /// </summary>
        public const string AllowFileExtensions = GroupName + ".AllowFileExtensions";

        public const int DefaultFileLimitLength = 100;
        public const string DefaultAllowFileExtensions = "dll,zip,rar,txt,log,xml,config,json,jpeg,jpg,png,bmp,ico,xlsx,xltx,xls,xlt,docs,dots,doc,dot,pptx,potx,ppt,pot,chm";
    }
}
