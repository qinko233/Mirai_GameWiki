using Mirai_CSharp.Models;
using System;
using System.Threading.Tasks;

#pragma warning disable CA1031 // Do not catch general exception types
namespace Mirai_CSharp.Example
{
    public partial class ExamplePlugin
    {
        public async Task<bool> Disconnected(MiraiHttpSession session, IDisconnectedEventArgs e)
        {
            // e.Exception: 引发掉线的响应异常, 按需处理
            MiraiHttpSessionOptions options = new MiraiHttpSessionOptions("47.242.52.67", 5200, "zheng12321");
            while (true)
            {
                try
                {
                    await session.ConnectAsync(options, 2033593308); // 连到成功为止, QQ号自填, 你也可以另行处理重连的 behaviour
                    return true;
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                }
            }
        }
    }
}
