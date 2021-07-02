using Mirai_GameWiki.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Mirai_GameWiki.Infrastructure
{
    public class PixivHelper
    {
        /// <summary>
        /// 访问URL
        /// </summary>
        /// <param name="reqUrl"></param>
        /// 
        /// <param name="method"></param>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public static string Pixiv(string reqUrl, string method, string paramData = "")
        {
            try
            {
                string post_key = string.Empty; //登录需要
                CookieContainer cookieContainer = new CookieContainer();//为了维持session和cookie
                #region 从登录页获取key
                string loginHtmlSrc = string.Empty;
                var loginPageUrl = "https://accounts.pixiv.net/login";
                HttpWebRequest request = WebRequest.Create(loginPageUrl) as HttpWebRequest;
                request.Method = "GET";
                request.CookieContainer = cookieContainer;
                request.Referer = "https://www.pixiv.net/";//必须设置 不然pixiv就不响应了
                request.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Mobile Safari/537.36";

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        stream.Close();
                        loginHtmlSrc = result;
                    }
                }
                post_key = new Regex("(?<=(name=\"post_key\"\\svalue=\"))(.*?)(?=\")").Match(loginHtmlSrc).Value;
                #endregion

                #region 登录
                string afterLoginHtmlSrc = string.Empty;
                string loginUrl = "https://accounts.pixiv.net/api/login?lang=zh";
                CookieCollection cookies = request.CookieContainer.GetCookies(new Uri(loginPageUrl));
                request = WebRequest.Create(loginUrl) as HttpWebRequest;
                var postData = JsonConvert.SerializeObject(new PixivLogin
                {
                    captcha = "",
                    g_recaptcha_response = "",
                    password = "QQzheng123",
                    pixiv_id = "museqinko@gmail.com",
                    post_key = post_key,
                    source = "pc",
                    Ref = "",
                    return_to = "https://www.pixiv.net/"
                });

                cookieContainer.Add(cookies);
                request.CookieContainer = cookieContainer;

                request.Method = "POST";
                request.Referer = "https://accounts.pixiv.net/login?return_to=https%3A%2F%2Fwww.pixiv.net%2F&lang=zh&source=pc&view_type=page";
                request.Accept = "application/json";
                request.ContentType = "application/x-www-form-urlencoded";
                #region 添加formData
                byte[] buffer = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                #endregion

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        stream.Close();
                        afterLoginHtmlSrc = result;
                    }
                }
                return afterLoginHtmlSrc;
                #endregion
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }

        }
    }
}
