using System;
using System.Collections.Generic;
using System.Text;

namespace Mirai_GameWiki.Model
{
    public class PixivLogin
    {
        public string pixiv_id { get; set; }
        public string password { get; set; }
        public string captcha { get; set; }
        public string g_recaptcha_response { get; set; }
        public string post_key { get; set; }
        public string source { get; set; }
        public string Ref { get; set; }
        public string return_to { get; set; }
    }
}
