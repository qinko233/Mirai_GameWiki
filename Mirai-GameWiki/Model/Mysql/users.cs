﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Mirai_GameWiki.Model.Mysql
{
    public partial class users
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public ulong Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [StringLength(500)]
        public string uname { get; set; }
    }
}