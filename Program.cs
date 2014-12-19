using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.Net.Security;
using System.IO;

namespace QCloud
{
    class Program
    {
        // SecretId 和 SecretKey
        private static string SECRET_ID = "SECRET_ID";
        private static string SECRET_KEY = "SECRET_KEY";

        static void Main(string[] args)
        {
            SortedDictionary<string, string> requestParams = new SortedDictionary<string, string>();
            requestParams.Add("SecretId", SECRET_ID);
            requestParams.Add("Region", "gz");
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            requestParams.Add("Timestamp", (unixTime / 1000).ToString());
            Random rand = new Random();
            requestParams.Add("Nonce", rand.Next(Int32.MaxValue).ToString());
            requestParams.Add("Action", "DescribeInstances");

            string requestMethod = "POST";
            string requestHost = "cvm.api.qcloud.com";
            string requestPath = "/v2/index.php";

            try 
            {
                string plainText = QCloudSign.MakeSignPlainText(requestParams, requestMethod, requestHost,requestPath);
                string sign = QCloudSign.Sign(plainText, SECRET_KEY);
                Console.WriteLine("原文: " + plainText);
                Console.WriteLine("签名: " + sign);
                if (requestMethod == "GET")
                    requestParams.Add("Signature", HttpUtility.UrlEncode(sign, Encoding.UTF8));
                else
                    requestParams.Add("Signature", sign);
                string retStr = SendRequest("https://" + requestHost + requestPath, requestParams, requestMethod);
                Console.WriteLine(retStr);
                Console.ReadKey();

            } catch(Exception e) {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        protected static string SendRequest(string url, SortedDictionary<string, string> requestParams, string requestMethod)
        {
            string paramStr = "";
            foreach (string key in requestParams.Keys)
            {
                paramStr += string.Format("{0}={1}&", key, requestParams[key]);
            }
            paramStr = paramStr.TrimEnd('&');
            if (requestMethod == "GET")
            {
                url += (url.IndexOf('?') > 0 ? "&" : "?") + paramStr;
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Accept = "*/*";
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1;SV1)";
            if (requestMethod == "POST")
            {
                byte[] data = Encoding.GetEncoding("utf-8").GetBytes(paramStr);
                request.ContentLength = data.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                using (Stream r = request.GetRequestStream())
                {
                    r.Write(data, 0, data.Length);
                }
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream s = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }
    }
}