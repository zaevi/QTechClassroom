using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QTechClassroom
{
    class URP
    {
        const string UrlHost = "http://222.195.242.240:8080";
        const string UrlCaptcha = "/validateCodeAction.do";
        const string UrlLogin = "/loginAction.do";
        const string UrlTest = "/bxqcjcxAction.do";
        const string UrlSpareClassrommPre = "/xszxcxAction.do?oper=xszxcx_lb";
        const string UrlSpareClassroom = "/xszxcxAction.do?oper=tjcx";

        public static HttpClient Web = null;
        public static CookieContainer CookieContainer = null;

        public static HttpClient Build()
        {
            CookieContainer = CookieContainer ?? new CookieContainer();

            var handler = new HttpClientHandler() { CookieContainer = CookieContainer };
            Web = new HttpClient(handler) { BaseAddress = new Uri(UrlHost) };
            return Web;
        }

        public static async Task<BitmapImage> GetCaptcha()
        {
            var response = await Web.GetAsync("/");
            var stream = await Web.GetStreamAsync("/validateCodeAction.do");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.EndInit();
                return imageSource;
            }
        }

        public static async Task<bool> Login(string user, string pass, string captcha)
        {
            var postData = PostData("zjh", user, "mm", pass, "v_yzm", captcha);
            var result = await Web.PostAsync("/loginAction.do", postData);
            bool success = await Test();
            return success;
        }

        public static async Task<bool> Test()
        {
            var stream = await Web.GetStreamAsync("/bxqcjcxAction.do");
            var doc = new HtmlDocument();
            doc.Load(stream);
            var node = doc.DocumentNode.SelectSingleNode("/html/head/title");
            return string.IsNullOrEmpty(node.InnerText);
        }

        public static async Task<IEnumerable<string>> GetSpareClassroom(string zxjc, string zxjxl, string zxxaq, string zxxnxq, string zxxq, string zxzc)
        {
            await Web.GetAsync(UrlSpareClassrommPre);
            var postData = PostData("currentPage", "1",
                "page", "1",
                "pageNo", "0",
                "pageSize", "300",
                "zxJc", zxjc, // 5,6,7,8
                "zxJxl", zxjxl, // 5001
                "zxXaq", zxxaq, // 05
                "zxxnxq", zxxnxq, // 2018-2019-1-1
                "zxxq", zxxq, // 2
                "zxZc", zxzc // 4
                );
            var stream = await Web.PostAsync(UrlSpareClassroom, postData);
            var doc = new HtmlDocument();
            doc.Load(await stream.Content.ReadAsStreamAsync());
            var list = doc.DocumentNode.SelectNodes("//*[@id=\"user\"]/tbody/tr").
                Select(n => n.SelectSingleNode("td[4]").InnerText).Select(t => t.Trim('\t', '\r', '\n'));
            return list;
        }

        public static FormUrlEncodedContent PostData(string key1, string value1, params string[] keyValues)
        {
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>(key1, value1));
            for (int i = 0; i < keyValues.Length; i += 2)
                collection.Add(new KeyValuePair<string, string>(keyValues[i], keyValues[i + 1]));
            return new FormUrlEncodedContent(collection);
        }
    }
}
