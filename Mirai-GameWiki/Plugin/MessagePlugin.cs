using Microsoft.Extensions.Configuration;
using Mirai_CSharp;
using Mirai_CSharp.Extensions;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
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
                    IMessageBuilder builder = GetReplyMsg(e, ref isReply, ref isAtSource);
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
                    IMessageBuilder builder = GetReplyMsg(e, ref isReply, ref isAtSource);
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
        /// 回复消息
        /// </summary>
        /// <param name="message">消息链[0]是消息主键,[1]开始是具体消息</param>
        /// <param name="isReply">是否回复消息</param>
        /// <param name="isAtSource">是否引用来源消息</param>
        /// <returns></returns>
        public IMessageBuilder GetReplyMsg(IGroupMessageEventArgs messageEvent, ref bool isReply, ref bool isAtSource)
        {
            isReply = true;
            isAtSource = false;
            IMessageBuilder builder = new MessageBuilder();
            var message = messageEvent.Chain;

            if (message.Length > 1)
            {
                var firstMsg = Convert.ToString(message[1]);
                //Type:Source、Plain、Face、Image
                switch (message[1].Type)
                {
                    case PlainMessage.MsgType:
                        #region 1.定义正则表达式
                        var getWikiReg = new Regex($"(?<=(^{GetJsonValue("BotName")}\\s+))([^(\\s)]+)$");//机器人名 词条名
                        var setWikiReg = new Regex($"(?<=(^{GetJsonValue("WikiCommand:Add")}\\s+))([^(\\s)]+)$");//添加词条 词条名
                        var removeWikiReg = new Regex($"(?<=(^{GetJsonValue("WikiCommand:Remove")}\\s+))([^(\\s)]+)$");//删除词条 词条名
                        #endregion

                        #region 2.1查百科
                        if (getWikiReg.IsMatch(firstMsg))
                        {
                            var question = getWikiReg.Match(firstMsg).Value;//词条的关键词
                            GetWiki(question, builder);
                        }
                        #endregion
                        #region 2.2添加词条关键词
                        else if (setWikiReg.IsMatch(firstMsg))
                        {
                            var question = setWikiReg.Match(firstMsg).Value;
                            if (!listenCache.ContainsKey(messageEvent.Sender.Id))
                            {
                                listenCache.Add(messageEvent.Sender.Id, question);
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
                        #region 2.4补充词条关键词对用内容
                        else
                        {
                            AddWiki(message, messageEvent.Sender.Id, builder);
                        }
                        #endregion
                        break;
                    case FaceMessage.MsgType:
                        isReply = false;
                        break;
                    case ImageMessage.MsgType:
                        AddWiki(message, messageEvent.Sender.Id, builder);
                        isReply = false;
                        break;
                    default:
                        isReply = false;
                        break;
                }
            }
            else
            {
                isReply = false;
            }
            return builder;
        }


        /// <summary>
        /// 回复消息
        /// </summary>
        /// <param name="message">消息链[0]是消息主键,[1]开始是具体消息</param>
        /// <param name="isReply">是否回复消息</param>
        /// <param name="isAtSource">是否引用来源消息</param>
        /// <returns></returns>
        public IMessageBuilder GetReplyMsg(IFriendMessageEventArgs messageEvent, ref bool isReply, ref bool isAtSource)
        {
            isReply = true;
            isAtSource = false;
            IMessageBuilder builder = new MessageBuilder();
            var message = messageEvent.Chain;

            if (message.Length > 1)
            {
                var firstMsg = Convert.ToString(message[1]);
                //Type:Source、Plain、Face、Image
                switch (message[1].Type)
                {
                    case PlainMessage.MsgType:
                        #region 1.定义正则表达式
                        var getWikiReg = new Regex($"(?<=(^{GetJsonValue("BotName")}\\s+))([^(\\s)]+)$");//机器人名 词条名
                        var setWikiReg = new Regex($"(?<=(^{GetJsonValue("WikiCommand:Add")}\\s+))([^(\\s)]+)$");//添加词条 词条名
                        var removeWikiReg = new Regex($"(?<=(^{GetJsonValue("WikiCommand:Remove")}\\s+))([^(\\s)]+)$");//删除词条 词条名
                        #endregion

                        #region 2.1查百科
                        if (getWikiReg.IsMatch(firstMsg))
                        {
                            var question = getWikiReg.Match(firstMsg).Value;//词条的关键词
                            GetWiki(question, builder);
                        }
                        #endregion
                        #region 2.2添加词条关键词
                        else if (setWikiReg.IsMatch(firstMsg))
                        {
                            var question = setWikiReg.Match(firstMsg).Value;
                            if (!listenCache.ContainsKey(messageEvent.Sender.Id))
                            {
                                listenCache.Add(messageEvent.Sender.Id, question);
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
                        #region 2.4补充词条关键词对用内容
                        else
                        {
                            AddWiki(message, messageEvent.Sender.Id, builder);
                        }
                        #endregion
                        break;
                    case FaceMessage.MsgType:
                        isReply = false;
                        break;
                    case ImageMessage.MsgType:
                        AddWiki(message, messageEvent.Sender.Id, builder);
                        isReply = false;
                        break;
                    default:
                        isReply = false;
                        break;
                }
            }
            else
            {
                isReply = false;
            }
            return builder;
        }

        /// <summary>
        /// 取command.json的值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public string GetJsonValue(string key) => _command[key];

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
                    builder.AddPlainMessage(string.Format(GetJsonValue("WikiCommand:NotFound"), GetJsonValue("WikiCommand:Add")));
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
        public void AddWiki(IMessageBase[] message, long senderId, IMessageBuilder builder)
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
                builder.AddPlainMessage(string.Format(GetJsonValue("WikiCommand:Thanks"), GetJsonValue("BotName")));
                //从监听数组里移除掉
                listenCache.Remove(senderId);
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
