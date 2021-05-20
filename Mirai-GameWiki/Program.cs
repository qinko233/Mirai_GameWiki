using Microsoft.Extensions.Configuration;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_GameWiki.Infrastructure;
using Mirai_GameWiki.Plugin;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mirai_GameWiki
{
    public static class Program
    {
        public static string configJsonFileName = "appsettings.json"; //项目配置文件
        public static string commandJsonFileName = "command.json"; //mirai指令文件
        public static IDatabase db;
        public static IConfiguration configuration;
        public static IConfiguration commandList;
        /// <summary>
        /// 配置
        /// </summary>
        public static void Configure()
        {
            try
            {
                #region 1.加载配置文件
                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), configJsonFileName)))
                {
                    File.Create(Path.Combine(Directory.GetCurrentDirectory(), configJsonFileName));
                }
                configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(configJsonFileName).Build();

                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), commandJsonFileName)))
                {
                    File.Create(Path.Combine(Directory.GetCurrentDirectory(), commandJsonFileName));
                }
                commandList = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(commandJsonFileName).Build();
                #endregion

                #region 2.配置数据库(redis)连接串
                var redis = configuration.GetSection("Redis");
                //连接字符串
                string _ip = redis.GetSection("Ip").Value;
                string _port = redis.GetSection("Port").Value;
                string _connectionString = $"{ _ip}:{_port}";

                //实例名称
                string _instanceName = redis.GetSection("InstanceName").Value;

                //默认数据库 
                int _defaultDB = int.Parse(redis.GetSection("DefaultDB").Value ?? "0");

                db = new RedisHelper(_connectionString, _instanceName, _defaultDB).GetDatabase();
                #endregion
            }
            catch
            {

            }
        }

        public static async Task Main()
        {
            Configure();

            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions(configuration["ip"], Convert.ToInt32(configuration["port"]), configuration["authKey"]);
            MessagePlugin plugin = new MessagePlugin(db, configuration, commandList);
            MiraiHttpSession session = new MiraiHttpSession();
            session.AddPlugin(plugin);
            // 使用上边提供的信息异步连接到 mirai-api-http
            await session.ConnectAsync(options, Convert.ToInt64(configuration["botQQ"])); //机器人QQ号  

            Console.WriteLine("服务启动~~");
            while (true)
            {
                if (await Console.In.ReadLineAsync() == "exit")
                {
                    return;
                }
            }
        }
    }
}
