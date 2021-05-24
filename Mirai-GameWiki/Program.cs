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
            string _ip = string.Empty;
            string _port = string.Empty;
            string _connectionString = string.Empty;
            string _instanceName = string.Empty;
            int _defaultDB = 0;

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
                _ip = redis.GetSection("Ip").Value;
                _port = redis.GetSection("Port").Value;
                _connectionString = $"{ _ip}:{_port}";

                //实例名称
                _instanceName = redis.GetSection("InstanceName").Value;

                //默认数据库 
                _defaultDB = int.Parse(redis.GetSection("DefaultDB").Value ?? "0");

                db = new RedisHelper(_connectionString, _instanceName, _defaultDB).GetDatabase();
                #endregion
            }
            catch (RedisConnectionException)
            {
                Console.WriteLine("redis连接失败,请检查");
                Console.WriteLine($"Ip:{_ip}  Port:{_port}");
                Console.WriteLine($"InstanceName:{_instanceName}");
                Console.WriteLine($"DefaultDB:{_defaultDB}");
                //Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("配置文件加载异常,请检查[*.json]: \r\n" + e.ToString());
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
                if (Console.In.ReadLine() == "exit")
                {
                    return;
                }
            }
        }
    }
}
