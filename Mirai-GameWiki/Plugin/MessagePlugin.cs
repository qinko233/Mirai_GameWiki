using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using Microsoft.Extensions.Configuration;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using Mirai_GameWiki.Infrastructure;
using Mirai_GameWiki.Model;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirai_GameWiki.Plugin
{
    public class MessagePlugin : IGroupMessage, IFriendMessage
    {
        public static IDatabase _db;
        public static IConfiguration _configuration;
        public static IConfiguration _command;
        public static Dictionary<long, string> listenCache;
        public static IInstaApi InstaApi;


        public MessagePlugin() { }

        public MessagePlugin(IDatabase db, IConfiguration configuration, IConfiguration command)
        {
            _db = db;
            _configuration = configuration;
            _command = command;
            listenCache = new Dictionary<long, string>();
        }

        public async Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            try
            {
                if (e.Chain.Length > 1)
                {
                    bool isReply = false, isAtSource = false;
                    IMessageBuilder builder = new MessageBuilder();
                    ReplyMessage(e.Chain, builder, e.Sender.Id, ref isReply);
                    if (isReply)//是否回复消息
                    {
                        if (isAtSource)//是否@消息源
                        {
                            await session.SendFriendMessageAsync(e.Sender.Id, builder, Convert.ToInt32(e.Chain[0].ToString()));
                        }
                        else
                        {
                            await session.SendFriendMessageAsync(e.Sender.Id, builder);
                        }
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            try
            {
                if (e.Chain.Length > 1)
                {
                    bool isReply = false, isAtSource = false;
                    IMessageBuilder builder = new MessageBuilder();
                    ReplyMessage(e.Chain, builder, e.Sender.Id, ref isReply);
                    if (isReply)//是否回复消息
                    {
                        if (isAtSource)//是否@消息源
                        {
                            await session.SendGroupMessageAsync(e.Sender.Group.Id, builder, Convert.ToInt32(e.Chain[0].ToString()));
                        }
                        else
                        {
                            await session.SendGroupMessageAsync(e.Sender.Group.Id, builder);
                        }
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        /// <summary>
        /// Q&A
        /// </summary>
        /// <param name="message"></param>
        /// <param name="builder"></param>
        /// <param name="senderId"></param>
        /// <param name="isReply"></param>
        public void ReplyMessage(IMessageBase[] message, IMessageBuilder builder, long senderId, ref bool isReply)
        {
            isReply = true;
            var firstMsg = Convert.ToString(message[1]);
            //Type:Source、Plain、Face、Image
            switch (message[1]?.Type)
            {
                case AtMessage.MsgType:
                case PlainMessage.MsgType:
                    firstMsg = message[1]?.Type == AtMessage.MsgType ? Convert.ToString(message[2]) : firstMsg;
                    #region 1.定义正则表达式
                    var getWikiReg = new Regex($"(?<=(^\\s*{_command["BotName"]}\\s+))([^(\\s)]+)$");//机器人名 词条名
                    var setWikiReg = new Regex($"(?<=(^\\s*{_command["WikiCommand:Add"]}\\s+))([^(\\s)]+)$");//添加词条 词条名
                    var removeWikiReg = new Regex($"(?<=(^\\s*{_command["WikiCommand:Remove"]}\\s+))([^(\\s)]+)$");//删除词条 词条名
                    var warframe_command = new Regex(_command["WarframeApi:command:regex"], RegexOptions.IgnoreCase);//warframe 指令
                    var warframe_sortie = new Regex(_command["WarframeApi:sortie:regex"], RegexOptions.IgnoreCase);//warframe 突击
                    var warframe_voidTrader = new Regex(_command["WarframeApi:voidTrader:regex"], RegexOptions.IgnoreCase);//warframe 突击
                    var warframe_cetusCycle = new Regex(_command["WarframeApi:cetusCycle:regex"], RegexOptions.IgnoreCase);//warframe 希图斯昼夜
                    var warframe_invasions = new Regex(_command["WarframeApi:invasions:regex"], RegexOptions.IgnoreCase);//warframe 入侵
                    var warframe_nightwave = new Regex(_command["WarframeApi:nightwave:regex"], RegexOptions.IgnoreCase);//warframe 午夜电波
                    var randomPicCommandReg = new Regex(string.Format(_command["RandomPicCommand:command:regex"], _command["BotName"]));//随机二次元图片
                    var helpReg = new Regex($"(?<=(\\s*{_command["BotName"]}\\s+))((帮助)|help)+$", RegexOptions.IgnoreCase);
                    #endregion

                    #region 2.0 帮助说明
                    if (helpReg.IsMatch(firstMsg))
                    {
                        builder.AddPlainMessage($"1. {_command["BotName"]}百科\r\n");
                        builder.AddPlainMessage($" 查询, 输入【{_command["BotName"]} “空格” 词条名】\r\n");
                        builder.AddPlainMessage($" 添加,【添加词条 词条名】触发指令，回复补充\r\n");
                        builder.AddPlainMessage("2. Warframe, 请输入【wf 指令】查询具体内容\r\n");
                        builder.AddPlainMessage($"3. 随机一张p站图(都是我关注哒),以下指令等价\r\n");
                        builder.AddPlainMessage($" 【{_command["BotName"]}来点二次元】\r\n 【{_command["BotName"]}二次元】\r\n 【来点二次元】\r\n");
                        builder.AddPlainMessage($"4. Instagram\r\n");
                        builder.AddPlainMessage($" 【/ins】随机图\r\n");
                        builder.AddPlainMessage($" 【/ins 用户id】指定用户随机图\r\n");
                        isReply = true;
                    }
                    #endregion
                    #region 2.1查百科
                    else if (getWikiReg.IsMatch(firstMsg))
                    {
                        var question = getWikiReg.Match(firstMsg).Value;//词条的关键词
                        GetWiki(question, builder);
                    }
                    #endregion
                    #region 2.2添加词条关键词
                    else if (setWikiReg.IsMatch(firstMsg))
                    {
                        var question = setWikiReg.Match(firstMsg).Value;
                        if (!listenCache.ContainsKey(senderId))
                        {
                            listenCache.Add(senderId, question);
                        }
                    }
                    #endregion
                    #region 2.3删除词条
                    else if (removeWikiReg.IsMatch(firstMsg))
                    {
                        var question = removeWikiReg.Match(firstMsg).Value;
                        builder.AddPlainMessage(RemoveWiki(question));
                    }
                    #endregion
                    #region 2.4 Warframe
                    #region 指令帮助
                    else if (warframe_command.IsMatch(firstMsg))
                    {
                        builder.AddPlainMessage($"以【wf、warframe、沃肥】开头\r\n 当前存在以下指令：\r\n");
                        var command_textList = _command.GetSection("WarframeApi:command:list").GetChildren().ToArray();
                        if (command_textList.Any())
                        {
                            command_textList.ToList().ForEach(m =>
                            {
                                builder.AddPlainMessage($"  {m.Value}\r\n");
                            });
                        }
                    }
                    #endregion
                    #region 突击
                    else if (warframe_sortie.IsMatch(firstMsg))
                    {
                        var url = _command["WarframeApi:Host"] + _command["WarframeApi:sortie:api"];
                        var result = JsonConvert.DeserializeObject<SortieModel>(HttpHelper.Send(url, "get"));
                        int i = 1;
                        builder.AddPlainMessage($"阵营：{result.faction}\r\n");
                        result.variants.ForEach(m =>
                        {
                            builder.AddPlainMessage($"突击{i}：\r\n  星球：{m.node}\r\n  任务：{m.missionType}\r\n  特性：{m.modifier}\r\n");
                            i++;
                        });
                        builder.AddPlainMessage($"boss：{result.boss}\r\n");
                        builder.AddPlainMessage($"剩余时间：{result.eta}");
                    }
                    #endregion
                    #region 奸商
                    else if (warframe_voidTrader.IsMatch(firstMsg))
                    {
                        var url = _command["WarframeApi:Host"] + _command["WarframeApi:voidTrader:api"];
                        var result = JsonConvert.DeserializeObject<VoidTraderModel>(HttpHelper.Send(url, "get"));
                        builder.AddPlainMessage($"地点：{result.location}\r\n");
                        builder.AddPlainMessage($"持续时间：{result.activation.AddHours(8).ToString("MM-dd HH:mm:ss")} -> {result.expiry.AddHours(8).ToString("MM-dd HH:mm:ss")}\r\n");
                        builder.AddPlainMessage($"剩余：{result.startString}到达，{result.endString}后离开");
                    }
                    #endregion
                    #region 希图斯昼夜
                    else if (warframe_cetusCycle.IsMatch(firstMsg))
                    {
                        var url = _command["WarframeApi:Host"] + _command["WarframeApi:cetusCycle:api"];
                        var result = JsonConvert.DeserializeObject<CetusCycleModel>(HttpHelper.Send(url, "get"));
                        builder.AddPlainMessage($"当前状态：{(result.isDay ? "白天" : "黑夜")}\r\n");
                        var time = new DateTime((result.expiry.AddHours(8) - DateTime.Now).Ticks);
                        builder.AddPlainMessage($"剩余时间：{time.ToString("HH小时mm分钟ss秒")}\r\n");
                        if (result.isDay && time.Minute < 20)
                        {
                            builder.AddPlainMessage($"  距离三傻不足20分钟了,请尽早准备!\r\n");
                        }
                    }
                    #endregion
                    #region 入侵
                    else if (warframe_invasions.IsMatch(firstMsg))
                    {
                        var url = _command["WarframeApi:Host"] + _command["WarframeApi:invasions:api"];
                        var str = HttpHelper.Send(url, "get");
                        var result = JsonConvert.DeserializeObject<List<InvasionsModel>>(str);
                        bool isTudou = false;
                        result.ForEach(m =>
                        {
                            //过滤掉已打完的
                            if (!m.completed)
                            {
                                #region 进攻方
                                string _msg__ = $"{(int)m.completion}%:{100 - (int)m.completion}% at {m.node} ";
                                if (m.attackerReward.countedItems.Any())
                                {
                                    var item = m.attackerReward.countedItems.FirstOrDefault();
                                    _msg__ += $"{item.type} x {item.count}";
                                    isTudou = new Regex("(Orokin)", RegexOptions.IgnoreCase).IsMatch(item.type);
                                }
                                else
                                {
                                    _msg__ += "无";
                                }
                                #endregion
                                _msg__ += " vs ";
                                #region 防守方
                                if (m.defenderReward.countedItems.Any())
                                {
                                    var item = m.defenderReward.countedItems.FirstOrDefault();
                                    _msg__ += $"{item.type} x {item.count}";
                                    isTudou = new Regex("(Orokin)", RegexOptions.IgnoreCase).IsMatch(item.type);
                                }
                                else
                                {
                                    _msg__ += "无";
                                }
                                builder.AddPlainMessage(_msg__ + "\r\n");
                                #endregion
                            }
                        });
                        if (isTudou) { builder.AddPlainMessage("  有土豆入侵，请尽早打!"); }
                    }
                    #endregion
                    #region 午夜电波
                    else if (warframe_nightwave.IsMatch(firstMsg))
                    {
                        var url = _command["WarframeApi:Host"] + _command["WarframeApi:nightwave:api"];
                        var result = JsonConvert.DeserializeObject<NightwaveModel>(HttpHelper.Send(url, "get"));
                        builder.AddPlainMessage($"时间：{result.activation.ToString("yyyy-MM-dd")}->{result.expiry.ToString("yyyy-MM-dd")}，阶段：{result.season}\r\n");
                        builder.AddPlainMessage("本周任务:\r\n");
                        result.activeChallenges.ForEach(m =>
                        {
                            builder.AddPlainMessage($" {(m.isDaily ? "[日常]" : (m.isElite ? "[精英]" : "[普通]"))}{m.reputation} {m.desc}\r\n");
                        });
                    }
                    #endregion
                    #endregion
                    #region 2.5 随机二次元图
                    else if (randomPicCommandReg.IsMatch(firstMsg))
                    {
                        using (var db = new MysqlHelper())
                        {
                            if (db.pixiv.Any())
                            {
                                int tryNum = 5;//自增id删了些图片导致不连续，所以5次查询机会
                                Model.Mysql.pixiv pic = null;
                                while (tryNum >= 0)
                                {
                                    var randomNum = new Random().Next(1, db.pixiv.Count());
                                    pic = db.pixiv.Where(m => m.id == randomNum).FirstOrDefault();
                                    if (pic != null)
                                    {
                                        builder.AddPlainMessage($"作者：{pic.userName}\r\n标题：{pic.title}\r\npid：{pic.pid}\r\n");
                                        builder.AddImageMessage(url: $"https://pixiv.cat/{pic.pid}.jpg");
                                        break;
                                    }
                                    tryNum--;
                                }
                            }
                            else
                            {
                                builder.AddPlainMessage($"糟糕, 作者删库跑路了 QAQ\r\n");
                            }
                            isReply = true;
                        }
                    }
                    #endregion
                    #region 2.6 Ins在线随机查询
                    //后续爬库添加离线版
                    else if (firstMsg.StartsWith("/ins"))
                    {
                        string msg = firstMsg.Replace("/ins", "");
                        if (string.IsNullOrEmpty(msg))
                        {
                            //关注列表随机
                            Task<ArrayList> al = InstaApiMedia.MediaRandomAsync();
                            al.Wait();

                            if (al.Result.Count == 2)
                            {
                                builder.AddPlainMessage($"Owner:{al.Result[0].ToString()}");
                                builder.AddImageMessage(url: al.Result[1].ToString());
                            }
                            else
                            {
                                builder.AddPlainMessage("try again");
                            }
                            isReply = true;
                        }
                        else
                        {
                            //添加查询人UserName
                            msg = new Regex("\\s").Replace(msg, "");
                            Task<ArrayList> al = InstaApiMedia.MediaUserRandomAsync(msg);
                            al.Wait();
                            if (al.Result.Count == 2)
                            {
                                builder.AddPlainMessage($"Owner:{al.Result[0].ToString()}");
                                builder.AddImageMessage(url: al.Result[1].ToString());
                            }
                            else
                            {
                                builder.AddPlainMessage("try again");
                            }
                            isReply = true;
                        }
                    }
                    #endregion
                    #region 2.7  本条目专门针对郭老板 
                    else if (senderId == 506717576 && !listenCache.ContainsKey(senderId))
                    {
                        isReply = true;
                        //5:00-23:00，仅限郭老板，含[到家了][人呢][来联盟]
                        if (DateTime.Now.Hour >= 5 && DateTime.Now.Hour <= 23 && new Regex("((((没|不)+去)|(在|到)家(了)?)|人呢|((来|打|玩)?联盟)|((下|落)班)|((放|休|请)假)|(来呀))").IsMatch(firstMsg))
                        {
                            string url = "https://wx2.sinaimg.cn/mw690/e9157a1fgy1grz9hfgx5wj209a0axjrh.jpg";
                            builder.AddImageMessage(url: url);
                        }
                        else if (new Regex("(摸鱼)").IsMatch(firstMsg))
                        {
                            builder.AddPlainMessage("有人正在摸，是谁我不说\n");
                            builder.AddAtMessage(senderId);
                        }
                        else if (new Regex("((肾)|(节制)|(危险)|(((se)|(色)|(涩))+图))").IsMatch(firstMsg))
                        {
                            builder.AddPlainMessage("你不对劲，你有问题\n");
                            builder.AddAtMessage(senderId);
                        }
                        else
                        {
                            isReply = false;
                        }
                    }
                    #endregion
                    #region 2.n 补充词条关键词对用内容
                    else
                    {
                        AddWiki(message, senderId, builder, ref isReply);
                    }
                    #endregion
                    break;
                case FaceMessage.MsgType:
                    isReply = false;
                    break;
                case ImageMessage.MsgType:
                    AddWiki(message, senderId, builder, ref isReply);
                    break;
                default:
                    isReply = false;
                    break;
            }
        }

        /// <summary>
        /// 查百科
        /// </summary>
        /// <param name="question">关键词</param>
        /// <returns></returns>
        public void GetWiki(string question, IMessageBuilder builder)
        {
            try
            {
                if (_db.KeyType(question).Equals(RedisType.String))
                {
                    builder.AddPlainMessage(_db.StringGet(question));
                }
                else if (_db.KeyType(question).Equals(RedisType.Hash))
                {
                    RedisValue[] kv = _db.HashValues(question);
                    foreach (var item in kv)
                    {
                        var answer = JsonConvert.DeserializeObject<MsgModel>(item);
                        if (answer.type == PlainMessage.MsgType)
                        {
                            builder.AddPlainMessage(answer.content);
                        }
                        else if (answer.type == ImageMessage.MsgType)
                        {
                            builder.AddImageMessage(url: answer.content);
                        }
                    }
                }
                else
                {
                    //未找到的提示信息
                    builder.AddPlainMessage(string.Format(_command["WikiCommand:NotFound"], _command["WikiCommand:Add"]) + "\r\n");

                    #region 相似词汇提醒
                    //redis模糊查询， redis-cli:keys *{question}*
                    var redisResult = _db.ScriptEvaluate(LuaScript.Prepare(
                                    //Redis的keys模糊查询：
                                    " local res = redis.call('KEYS', @keypattern) return res "), new { @keypattern = $"*{question}*" });
                    if (!redisResult.IsNull)
                    {
                        builder.AddPlainMessage("===以下为相似关键词===\r\n");
                        foreach (var dic in (string[])redisResult)
                        {
                            builder.AddPlainMessage($" {dic}\r\n");
                        }
                    }
                    #endregion
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 定义百科关键词
        /// </summary>
        /// <param name="question">关键词</param>
        /// <param name="answer">内容</param>
        public void SetWiki(string question, List<MsgModel> answer)
        {
            try
            {
                int i = 0;
                answer.ForEach(m =>
                {
                    _db.HashSet(new RedisKey(question), i, JsonConvert.SerializeObject(answer[i]));
                    i++;
                });
            }
            catch
            {

            }
        }

        /// <summary>
        /// 补充百科内容
        /// </summary>
        /// <param name="message"></param>
        /// <param name="senderId"></param>
        /// <param name="builder"></param>
        public void AddWiki(IMessageBase[] message, long senderId, IMessageBuilder builder, ref bool isReply)
        {
            string question;
            if (listenCache.TryGetValue(senderId, out question))
            {
                List<MsgModel> answer = new List<MsgModel>();
                for (int i = 1; i < message.Length; i++)
                {
                    if (message[i].Type == PlainMessage.MsgType)
                    {
                        answer.Add(new MsgModel() { type = message[i].Type, content = message[i].ToString() });
                    }
                    else if (message[i].Type == ImageMessage.MsgType)
                    {
                        answer.Add(new MsgModel() { type = message[i].Type, content = (message[i] as ImageMessage).Url });
                    }
                }
                SetWiki(question, answer);
                //添加欢迎信息
                builder.AddPlainMessage(string.Format(_command["WikiCommand:Thanks"], _command["BotName"]));
                //从监听数组里移除掉
                listenCache.Remove(senderId);
            }
            else
            {
                isReply = false;
            }
        }

        /// <summary>
        /// 删除百科
        /// </summary>
        /// <param name="question">关键词</param>
        /// <returns></returns>
        public string RemoveWiki(string question)
        {
            try
            {
                if (_db.KeyDelete(question))
                {
                    return "删除成功!";
                }
                else
                {
                    return "不存在该词条";
                }
            }
            catch
            {
                return "";
            }
        }

    }

    public class MsgModel
    {
        public string type { get; set; }
        public string content { get; set; }
    }
}
