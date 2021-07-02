using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Mirai_GameWiki.Model.Mysql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mirai_GameWiki.Infrastructure
{
    public class MysqlHelper : qqbotContext
    {
        public IConfiguration _configuration;
        public static string configJsonFileName = "appsettings.json"; //项目配置文件

        public MysqlHelper()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(configJsonFileName).Build();
        }

        public MysqlHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MysqlHelper(DbContextOptions<qqbotContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(_configuration.GetSection("mysql").GetSection("ConnectionStrings")["Default"], Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.25-mysql"));
            }
        }
    }
}
