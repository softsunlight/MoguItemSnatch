using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoguItemSnatch
{
    /// <summary>
    /// 获取蘑菇街商品工具类
    /// </summary>
    public class MoguUtil
    {

        private static string uuid;

        private static string h5Token;

        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <returns></returns>
        private static string GetUuid()
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://portal.mogu.com/api/util/getUuid?callback=callback_1001");
                httpWebRequest.Method = "GET";
                httpWebRequest.Referer = "https://www.mogu.com/";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                string cookies = httpWebResponse.GetResponseHeader("Set-Cookie");
                string uuids = string.Empty;
                MatchCollection mc = Regex.Matches(cookies, @"__mgjuuid=(?<mgjuuid>[^;]*)");
                foreach (Match m in mc)
                {
                    if (m.Success)
                    {
                        uuids = m.Groups["mgjuuid"].Value;
                    }
                }
                return uuids;
            }
            catch (Exception ex)
            {
                Log.Write("MoguUtil.GetUuid Error", ex);
            }
            finally
            {
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                    httpWebResponse.Dispose();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
            return string.Empty;
        }

        public static string GetH5Token()
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
                uuid = GetUuid();
                httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.mogu.com/h5/mwp.darwin.multiget/3/?data={\"pids\":\"132244, 138852, 138851\"}\"&mw-appkey=100028&mw-ttid=NMMain@mgj_pc_1.0&mw-t=" + GetUnixTimeStamp(DateTime.Now) + "&mw-uuid=" + uuid + "&mw-h5-os=unknown&mw-sign=&callback=mwpCb1&_=" + GetUnixTimeStamp(DateTime.Now));
                httpWebRequest.Method = "GET";
                httpWebRequest.Referer = "https://www.mogu.com/";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                string cookies = httpWebResponse.GetResponseHeader("Set-Cookie");
                string mwpH5Token = string.Empty;
                MatchCollection mc = Regex.Matches(cookies, @"_mwp_h5_token=(?<mwpH5Token>[^;]*)");
                foreach (Match m in mc)
                {
                    if (m.Success)
                    {
                        mwpH5Token = m.Groups["mwpH5Token"].Value;
                    }
                }
                return mwpH5Token;
            }
            catch (Exception ex)
            {
                Log.Write("MoguUtil.GetH5Token Error", ex);
            }
            finally
            {
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                }
                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                    httpWebResponse.Dispose();
                }
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
            }
            return string.Empty;
        }

        private static long GetUnixTimeStamp(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

    }
}
