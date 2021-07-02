using Mirai_GameWiki.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Mirai_GameWiki.Infrastructure
{
    public class HttpHelper
    {
        /// <summary>
        /// 访问URL
        /// </summary>
        /// <param name="reqUrl"></param>
        /// <param name="method"></param>
        /// <param name="paramData"></param>
        /// <returns></returns>
        public static string Send(string reqUrl, string method, string paramData = "")
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
                request.Method = method.ToUpperInvariant();
                request.Proxy = null;

                if (!"GET".Equals(request.Method) && !string.IsNullOrEmpty(paramData) && paramData.Length > 0)
                {
                    request.ContentType = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(paramData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        stream.Close();
                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                return ex.Message;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}
