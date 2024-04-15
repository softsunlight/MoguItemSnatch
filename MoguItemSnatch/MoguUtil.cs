using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web;

namespace MoguItemSnatch
{
    /// <summary>
    /// 获取蘑菇街商品工具类
    /// </summary>
    public class MoguUtil
    { 

        private static string uuid;

        private static string h5Token;

        private static string _cookies;

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
                _cookies += "__mgjuuid=" + uuids + ";";
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

        /// <summary>
        /// 获取h5_token值
        /// </summary>
        /// <returns></returns>
        private static string GetH5Token()
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
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
                _cookies += "_mwp_h5_token=" + mwpH5Token + ";";
                string mwpH5TokenEnc = string.Empty;
                mc = Regex.Matches(cookies, @"_mwp_h5_token_enc=(?<mwpH5TokenEnc>[^;]*)");
                foreach (Match m in mc)
                {
                    if (m.Success)
                    {
                        mwpH5TokenEnc = m.Groups["mwpH5TokenEnc"].Value;
                    }
                }
                _cookies += "_mwp_h5_token_enc=" + mwpH5TokenEnc + ";";
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

        /// <summary>
        /// 获取蘑菇街商品数据
        /// </summary>
        /// <param name="moguItemId">蘑菇街商品ID</param>
        /// <returns></returns>
        public static string GetItemData(string moguItemId)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
                if (string.IsNullOrEmpty(uuid))
                {
                    uuid = GetUuid();
                }
                if (string.IsNullOrEmpty(h5Token))
                {
                    h5Token = GetH5Token();
                }

                string itemUrl = "https://api.mogu.com/h5/http.detail.api/1/";

                Dictionary<string, string> requestParams = new Dictionary<string, string>();
                requestParams["mw-appkey"] = "100028";
                requestParams["mw-ttid"] = "NMMain@mgj_pc_1.0";
                requestParams["mw-t"] = GetUnixTimeStamp(DateTime.Now).ToString();
                requestParams["mw-uuid"] = uuid;
                requestParams["mw-h5-os"] = "unknown";

                var orderedResult = requestParams.Keys.OrderBy(x => x);

                string signContent = string.Join("&", orderedResult.Select(p => requestParams[p]));

                signContent += "&http.detail.api";
                signContent += "&1";
                string data = "{\"iid\":\"" + moguItemId + "\",\"activityId\":\"\",\"fastbuyId\":\"\",\"template\":\"1-1-detail_normal-1.0.0\"}";
                signContent += "&" + Md5(data);
                signContent += "&" + h5Token;

                requestParams["data"] = data;

                requestParams["mw-sign"] = Md5(signContent);
                requestParams["callback"] = "mwpCb2";
                requestParams["_"] = GetUnixTimeStamp(DateTime.Now).ToString();


                httpWebRequest = (HttpWebRequest)WebRequest.Create(itemUrl + "?" + string.Join("&", requestParams.Keys.Select(p => p + "=" + requestParams[p])));
                httpWebRequest.Method = "GET";
                httpWebRequest.Referer = "https://shop.mogu.com/detail/" + moguItemId;
                httpWebRequest.Headers["cookie"] = _cookies;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                result = result.Replace("mwpCb2(", "");
                result = result.TrimEnd(')');
                return result;
            }
            catch (Exception ex)
            {
                Log.Write("MoguUtil.GetItemData Error", ex);
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

        /// <summary>
        /// 获取蘑菇街主题市场中分类的商品数据
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static string GetSearchData(string action, int pageNo, int pageSize)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
                string itemUrl = "https://list.mogu.com/search";

                Dictionary<string, string> requestParams = new Dictionary<string, string>();
                requestParams["callback"] = "searchdatacallback";
                requestParams["cKey"] = pageSize.ToString();
                requestParams["page"] = pageNo.ToString();
                requestParams["sort"] = "pop";
                requestParams["action"] = action;
                requestParams["_"] = GetUnixTimeStamp(DateTime.Now).ToString();


                httpWebRequest = (HttpWebRequest)WebRequest.Create(itemUrl + "?" + string.Join("&", requestParams.Keys.Select(p => p + "=" + requestParams[p])));
                httpWebRequest.Method = "GET";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                return result;
            }
            catch (Exception ex)
            {
                Log.Write("MoguUtil.GetSearchData Error", ex);
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

        /// <summary>
        /// 获取蘑菇街店铺内所有商品数据
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static string GetShopData(string shopId, int pageNo, int pageSize)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            StreamReader streamReader = null;
            try
            {
                if (string.IsNullOrEmpty(uuid))
                {
                    uuid = GetUuid();
                }
                if (string.IsNullOrEmpty(h5Token))
                {
                    h5Token = GetH5Token();
                }

                string itemUrl = "https://api.mogu.com/h5/mwp.shopappservice.goodsAll/1/";

                Dictionary<string, string> requestParams = new Dictionary<string, string>();
                requestParams["mw-appkey"] = "100028";
                requestParams["mw-ttid"] = "NMMain@mgj_pc_1.0";
                requestParams["mw-t"] = GetUnixTimeStamp(DateTime.Now).ToString();
                requestParams["mw-uuid"] = uuid;
                requestParams["mw-h5-os"] = "unknown";

                var orderedResult = requestParams.Keys.OrderBy(x => x);

                string signContent = string.Join("&", orderedResult.Select(p => requestParams[p]));

                signContent += "&mwp.shopappservice.goodsAll";
                signContent += "&1";
                string data = "{\"shopId\":\"" + shopId + "\",\"categoryId\":\"\",\"page\":" + pageNo + ",\"shopType\":\"mgjpc\",\"pageSize\":" + pageSize + ",\"sort\":\"\"}";
                signContent += "&" + Md5(data);
                signContent += "&" + h5Token;

                requestParams["data"] = data;

                requestParams["mw-sign"] = Md5(signContent);
                requestParams["callback"] = "mwpCb2";
                requestParams["_"] = GetUnixTimeStamp(DateTime.Now).ToString();


                httpWebRequest = (HttpWebRequest)WebRequest.Create(itemUrl + "?" + string.Join("&", requestParams.Keys.Select(p => p + "=" + requestParams[p])));
                httpWebRequest.Method = "GET";
                httpWebRequest.Referer = "https://shop.mogu.com/" + shopId + "/index/total";
                httpWebRequest.Headers["cookie"] = _cookies;
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                result = result.Replace("mwpCb2(", "");
                result = result.TrimEnd(')');
                return result;
            }
            catch (Exception ex)
            {
                Log.Write("MoguUtil.GetShopData Error", ex);
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

        /// <summary>
        /// 获取Unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static long GetUnixTimeStamp(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 获取MD5值
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string Md5(string content)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "").ToLower();
            }
        }

    }
}
