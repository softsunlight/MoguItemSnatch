using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using MoguItemSnatch.Domain.Item;

namespace MoguItemSnatch
{
    /// <summary>
    /// 蘑菇街商品爬虫
    /// </summary>
    public class MoguCrawler
    {
        /// <summary>
        /// 当前获取蘑菇街主题市场商品数据进度保存文件
        /// </summary>
        private string currentSearchProgessSaveFileName = "moguisearchdata.data";
        /// <summary>
        /// 蘑菇街商品ID保存文件
        /// </summary>
        private string itemSaveFileName = "moguitemids.data";
        /// <summary>
        /// 蘑菇街店铺ID保存文件
        /// </summary>
        private string shopSaveFileName = "mogushopids.data";
        /// <summary>
        /// 蘑菇街主题名称列表
        /// </summary>
        private List<string> actionList = new List<string>() { "clothing", "skirt", "trousers", "neiyi", "shoes", "bags", "boyfriend", "baby", "home", "accessories", "jiadian", "food" };
        /// <summary>
        /// 商品ID集合
        /// </summary>
        private HashSet<string> itemIdSet;
        /// <summary>
        /// 上次保存商品ID集合时的数量
        /// </summary>
        private int lastCount = 0;
        /// <summary>
        /// 商品ID正则表达式
        /// </summary>
        private Regex itemIdReg = new Regex(@"(?is)""tradeItemId""\s*:\s*""(?<itemId>[^""]*)""");
        /// <summary>
        /// 启动爬虫
        /// </summary>
        public void Start()
        {

            Task.Run(() =>
            {
                SnatchItemDetail();
            });

            //先恢复已保存的数据
            LoadExistedItemId();

            int startIndex = 0;
            int pageNo = 1;
            int pageSize = 15;
            int maxPageNo = 100;
            string action = string.Empty;
            LoadCurrentSearchData(out action, out pageNo);
            if (!string.IsNullOrEmpty(action))
            {
                startIndex = actionList.FindIndex(x => x == action);
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            if (pageNo <= 0)
            {
                pageNo = 1;
            }

            while (true)
            {
                for (var i = startIndex; i < actionList.Count; i++)
                {
                    while (true)
                    {
                        FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentSearchProgessSaveFileName), actionList[i] + "|" + pageNo);

                        string result = MoguUtil.GetSearchData(actionList[i], pageNo, pageSize);
                        ProcessData(result);
                        int count = itemIdSet.Count;
                        if (count != lastCount)
                        {
                            List<string> tempList = new List<string>(itemIdSet);
                            if (tempList != null && tempList.Count > 0)
                            {
                                //保存
                                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, itemSaveFileName), string.Join(Environment.NewLine, tempList));
                            }
                            lastCount = count;
                        }
                        if (pageNo >= maxPageNo)
                        {
                            pageNo = 1;
                            break;
                        }
                        pageNo++;
                        Thread.Sleep(50);
                    }
                }
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentSearchProgessSaveFileName));
                Thread.Sleep(24 * 60 * 60 * 1000);
            }
        }

        private void LoadExistedItemId()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, itemSaveFileName);
            if (File.Exists(filePath))
            {
                string content = string.Empty;
                bool isOk = FileUtil.Read(filePath, out content);
                //to do 文件读取失败时要做处理
                if (isOk && !string.IsNullOrEmpty(content))
                {
                    itemIdSet = new HashSet<string>(content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }

        private void LoadCurrentSearchData(out string action, out int pageNo)
        {
            action = string.Empty;
            pageNo = 1;
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentSearchProgessSaveFileName);
            if (File.Exists(filePath))
            {
                string content = string.Empty;
                bool isOk = FileUtil.Read(filePath, out content);
                //to do 文件读取失败时要做处理
                if (isOk && !string.IsNullOrEmpty(content))
                {
                    string[] datas = content.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    if (datas.Length >= 2)
                    {
                        action = datas[0];
                        try
                        {
                            pageNo = Convert.ToInt32(datas[1]);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        private void ProcessData(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                int count = 0;
                MatchCollection matches = itemIdReg.Matches(content);
                foreach (Match m in matches)
                {
                    if (m.Success)
                    {
                        string itemId = m.Groups["itemId"].Value;
                        if (!string.IsNullOrEmpty(itemId))
                        {
                            if (itemIdSet == null)
                            {
                                itemIdSet = new HashSet<string>();
                            }
                            itemIdSet.Add(itemId);
                            count++;
                        }
                    }
                }

            }
        }

        private void SnatchItemDetail()
        {
            while (true)
            {
                try
                {
                    while (itemIdSet.Count > 0)
                    {
                        string itemId = itemIdSet.ElementAt(0);
                        string itemStr = MoguUtil.GetItemData(itemId);
                        HttpDetailApiResponse itemDetailResponse = JsonConvert.DeserializeObject<HttpDetailApiResponse>(itemStr);
                        if (itemDetailResponse != null && itemDetailResponse.Data != null && itemDetailResponse.Data.Result != null)
                        {
                            ShopInfo shopInfo = itemDetailResponse.Data.Result.ShopInfo;
                            if (shopInfo != null)
                            {

                            }
                        }
                        itemIdSet.Remove(itemId);
                        Thread.Sleep(20);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchItemDetail Error", ex);
                }
            }
        }

    }
}
