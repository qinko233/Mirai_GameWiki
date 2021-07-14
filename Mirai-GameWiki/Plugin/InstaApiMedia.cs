using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mirai_GameWiki.Plugin
{
    public static class InstaApiMedia
    {

        static IInstaApi InstaApi;

        //登录账号
        public static async System.Threading.Tasks.Task InsLoginAsync()
        {
            #region 登录Ins
            Console.WriteLine("登录Ins...");
            var userSession = new UserSessionData
            {
                UserName = "824559791@qq.com",
                Password = "Alroy1995"
            };

            var delay = RequestDelay.FromSeconds(2, 2);
            InstaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All))
                .SetRequestDelay(delay)
                .Build();
            const string stateFile = "state.bin";
            try
            {
                if (System.IO.File.Exists(stateFile))
                {
                    Console.WriteLine("从文件加载登录.");
                    using (var fs = System.IO.File.OpenRead(stateFile))
                    {
                        InstaApi.LoadStateDataFromStream(fs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (!InstaApi.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                delay.Disable();
                var logInResult = await InstaApi.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                }
            }
            var state = InstaApi.GetStateDataAsStream();
            using (var fileStream = System.IO.File.Create(stateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
            Console.WriteLine("已登录");
            #endregion
        }

        //关注列表随机用户随即图片
        public static async System.Threading.Tasks.Task<ArrayList> MediaRandomAsync()
        {
            await InsLoginAsync();
            #region 随机关注列表随机图片
            ArrayList al = new ArrayList();
            Random rd = new Random();
            string mediaurl;
            string followUserName;
            var currentUserFollowing = await InstaApi.UserProcessor.GetUserFollowingAsync("alroy233", PaginationParameters.MaxPagesToLoad(20));
            if (currentUserFollowing.Info.ResponseType.ToString().Equals("OK"))
            {
                if (currentUserFollowing.Value.Count > 0)
                {
                    int funNo = rd.Next(1, currentUserFollowing.Value.Count);
                    followUserName = currentUserFollowing.Value[funNo].UserName;

                    var currentUserMedia = await InstaApi.UserProcessor.GetUserMediaAsync(followUserName, PaginationParameters.MaxPagesToLoad(0));
                    var userMediaValue = currentUserMedia.Value;
                    if (currentUserMedia.Info.ResponseType.ToString() == "OK" && userMediaValue.Count > 0)
                    {
                        int mediaNo = rd.Next(0, userMediaValue.Count - 1);
                        if (userMediaValue[mediaNo].MediaType.ToString() == "Carousel")//如果是相册
                        {
                            int carouselNo = rd.Next(1, userMediaValue[mediaNo].Carousel.Count);
                            mediaurl = userMediaValue[mediaNo].Carousel[carouselNo].Images[0].Uri;
                            Console.WriteLine(followUserName + "   Uri" + mediaurl);
                            //SendInfo(4, e, "", "", mediaurl);
                            al.Add(followUserName);
                            al.Add(mediaurl);
                            return al;
                        }
                        else if (userMediaValue[mediaNo].MediaType.ToString() == "Image")//如果是图片
                        {
                            mediaurl = userMediaValue[mediaNo].Images[0].Uri;
                            Console.WriteLine(followUserName + "   Uri" + mediaurl);
                            //SendInfo(4, e, "", "", mediaurl);
                            al.Add(followUserName);
                            al.Add(mediaurl);
                            return al;
                        }
                        else
                        {
                            return al;
                        }
                    }
                    else
                    {
                        Console.WriteLine("未查询到用户" + followUserName);
                        return al;
                    }
                }
                else
                {
                    return al;
                }
            }
            else
            {
                return al;
            }
            #endregion
        }

        //指定用户名随机图片
        public static async System.Threading.Tasks.Task<ArrayList> MediaUserRandomAsync(string userName)
        {
            await InsLoginAsync();
            #region 随机关注列表随机图片
            ArrayList al = new ArrayList();
            Random rd = new Random();
            string mediaurl;

            var currentUserMedia = await InstaApi.UserProcessor.GetUserMediaAsync(userName, PaginationParameters.MaxPagesToLoad(0));
            var userMediaValue = currentUserMedia.Value;
            if (currentUserMedia.Info.ResponseType.ToString() == "OK" && userMediaValue.Count > 0)
            {
                int mediaNo = rd.Next(0, userMediaValue.Count - 1);
                if (userMediaValue[mediaNo].MediaType.ToString() == "Carousel")//如果是相册
                {
                    int carouselNo = rd.Next(1, userMediaValue[mediaNo].Carousel.Count);
                    mediaurl = userMediaValue[mediaNo].Carousel[carouselNo].Images[0].Uri;
                    Console.WriteLine(userName + "   Uri" + mediaurl);
                    //SendInfo(4, e, "", "", mediaurl);
                    al.Add(userName);
                    al.Add(mediaurl);
                    return al;
                }
                else if (userMediaValue[mediaNo].MediaType.ToString() == "Image")//如果是图片
                {
                    mediaurl = userMediaValue[mediaNo].Images[0].Uri;
                    Console.WriteLine(userName + "   Uri" + mediaurl);
                    //SendInfo(4, e, "", "", mediaurl);
                    al.Add(userName);
                    al.Add(mediaurl);
                    return al;
                }
                else
                {
                    return al;
                }
            }
            else
            {
                Console.WriteLine("未查询到用户" + userName);
                return al;
            }

            #endregion
        }
    }
}
