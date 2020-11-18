﻿using System.ComponentModel.DataAnnotations;

namespace Agile.Abp.FileManagement
{
    public class FileSystemGetDto
    {
        [StringLength(255)]
        public string Path { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
    }
}
