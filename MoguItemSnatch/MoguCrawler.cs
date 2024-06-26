﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using MoguItemSnatch.Domain.Item;
using MoguItemSnatch.Model;
using MoguItemSnatch.Dao;
using System.Web;

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
        /// 蘑菇街目录商品商品ID保存文件
        /// </summary>
        private string searchItemSaveFileName = "mogusearchitemids.data";
        /// <summary>
        /// 蘑菇街店铺商品商品ID保存文件
        /// </summary>
        private string shopItemSaveFileName = "mogushopitemids.data";
        /// <summary>
        /// 蘑菇街店铺ID保存文件
        /// </summary>
        private string shopSaveFileName = "mogushopids.data";
        /// <summary>
        /// 蘑菇街主题名称列表
        /// </summary>
        private List<string> actionList = new List<string>() { "clothing", "skirt", "trousers", "neiyi", "shoes", "bags", "boyfriend", "baby", "home", "accessories", "jiadian", "food" };
        /// <summary>
        /// 搜索商品ID集合
        /// </summary>
        private HashSet<string> searchItemIdSet = new HashSet<string>();
        /// <summary>
        /// 搜索商品ID集合锁
        /// </summary>
        private static readonly object searchItemIdSetLockObj = new object();
        /// <summary>
        /// 店铺所有商品ID集合
        /// </summary>
        private HashSet<string> shopItemIdSet = new HashSet<string>();
        /// <summary>
        /// 店铺所有商品ID集合锁
        /// </summary>
        private static readonly object shopItemIdSetLockObj = new object();
        /// <summary>
        /// 上次保存商品ID集合时的数量
        /// </summary>
        private int lastCount = 0;
        /// <summary>
        /// 商品ID正则表达式
        /// </summary>
        private Regex itemIdReg = new Regex(@"(?is)""tradeItemId""\s*:\s*""(?<itemId>[^""]*)""");
        /// <summary>
        /// 店铺ID集合
        /// </summary>
        private HashSet<string> shopIdSet = new HashSet<string>();
        /// <summary>
        /// 店铺ID集合锁
        /// </summary>
        private static readonly object shopIdSetLockObj = new object();
        /// <summary>
        /// 启动爬虫
        /// </summary>
        public void Start()
        {

            //先恢复已保存的数据
            LoadExistedSearchItemId();

            LoadExistedShopItemId();

            LoadExistedShopId();

            Task.Run(() =>
            {
                //采集商品详情
                SnatchSearchItemDetail();
            });

            Task.Run(() =>
            {
                //采集商品详情
                SnatchShopItemDetail();
            });

            Task.Run(() =>
            {
                //采集店铺所有商品
                SnatchShopItem();
            });

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
                        int count = searchItemIdSet.Count;
                        if (count != lastCount)
                        {
                            lock (searchItemIdSetLockObj)
                            {
                                List<string> tempList = new List<string>(searchItemIdSet);
                                if (tempList != null && tempList.Count > 0)
                                {
                                    //保存
                                    FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchItemSaveFileName), string.Join(Environment.NewLine, tempList));
                                }
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

        /// <summary>
        /// 启动爬虫
        /// </summary>
        public void Start2()
        {
            Task.Run(() =>
            {
                //采集商品详情
                SnatchShopItemDetail();
            });

            Task.Run(() =>
            {
                //采集店铺所有商品
                SnatchShopItem();
            });

            while (true)
            {
                try
                {
                    int len = 6;
                    string str = "0123456789abcdefghijklmnopqrstuvwxyz";
                    StringBuilder str2 = new StringBuilder();
                    for (int i = 0; i < len; i++)
                    {
                        str2.Append(str[new Random().Next(str.Length)]);
                    }
                    lock (shopItemIdSetLockObj)
                    {
                        shopItemIdSet.Add($"{(DateTime.Now.Ticks % 10 == 1 ? new Random().Next(2, 10) : 1)}{str2}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchSearchItemDetail Error", ex);
                }
            }
        }

        private void LoadExistedSearchItemId()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchItemSaveFileName);
            if (File.Exists(filePath))
            {
                string content = string.Empty;
                bool isOk = FileUtil.Read(filePath, out content);
                //to do 文件读取失败时要做处理
                if (isOk && !string.IsNullOrEmpty(content))
                {
                    searchItemIdSet = new HashSet<string>(content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    searchItemIdSet = new HashSet<string>();
                }
            }
            else
            {
                searchItemIdSet = new HashSet<string>();
            }
        }

        private void LoadExistedShopItemId()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopItemSaveFileName);
            if (File.Exists(filePath))
            {
                string content = string.Empty;
                bool isOk = FileUtil.Read(filePath, out content);
                //to do 文件读取失败时要做处理
                if (isOk && !string.IsNullOrEmpty(content))
                {
                    shopItemIdSet = new HashSet<string>(content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    shopItemIdSet = new HashSet<string>();
                }
            }
            else
            {
                shopItemIdSet = new HashSet<string>();
            }
        }

        private void LoadExistedShopId()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName);
            if (File.Exists(filePath))
            {
                string content = string.Empty;
                bool isOk = FileUtil.Read(filePath, out content);
                //to do 文件读取失败时要做处理
                if (isOk && !string.IsNullOrEmpty(content))
                {
                    shopIdSet = new HashSet<string>(content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    shopIdSet = new HashSet<string>();
                }
            }
            else
            {
                shopIdSet = new HashSet<string>();
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
                            lock (searchItemIdSetLockObj)
                            {
                                if (searchItemIdSet == null)
                                {
                                    searchItemIdSet = new HashSet<string>();
                                }
                                searchItemIdSet.Add(itemId);
                            }
                            count++;
                        }
                    }
                }

            }
        }

        private void SnatchSearchItemDetail()
        {
            while (true)
            {
                try
                {
                    if (searchItemIdSet.Count > 0)
                    {
                        List<string> itemList = null;
                        lock (searchItemIdSetLockObj)
                        {
                            itemList = new List<string>(searchItemIdSet);
                        }
                        int maxTaskCount = 10;
                        int usedTaskCount = 0;
                        int index = 1;
                        List<string> itemIdList = new List<string>();
                        object usedTaskLockObj = new object();
                        foreach (string itemId in itemList)
                        {
                            itemIdList.Add(itemId);
                            if (index % 20 == 0 || index >= itemList.Count)
                            {
                                while (usedTaskCount >= maxTaskCount)
                                {
                                    Thread.Sleep(1000);
                                }
                                usedTaskCount++;
                                List<string> tempList = new List<string>(itemIdList);
                                itemIdList.Clear();
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        if (tempList != null)
                                        {
                                            foreach (string tempItemId in tempList)
                                            {
                                                SaveItemDetail(tempItemId, true);
                                                lock (searchItemIdSetLockObj)
                                                {
                                                    searchItemIdSet.Remove(tempItemId);
                                                }
                                            }
                                            lock (searchItemIdSetLockObj)
                                            {
                                                //保存
                                                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchItemSaveFileName), string.Join(Environment.NewLine, new List<string>(searchItemIdSet)));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    finally
                                    {
                                        lock (usedTaskLockObj)
                                        {
                                            usedTaskCount--;
                                        }
                                    }
                                });
                            }
                            index++;
                            //SaveItemDetail(itemId);
                            //itemIdSet.Remove(itemId);
                            //Thread.Sleep(20);
                        }
                        while (usedTaskCount > 0)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchSearchItemDetail Error", ex);
                }
            }
        }

        private void SnatchShopItemDetail()
        {
            while (true)
            {
                try
                {
                    if (shopItemIdSet.Count > 0)
                    {
                        List<string> itemList = null;
                        lock (shopItemIdSetLockObj)
                        {
                            itemList = new List<string>(shopItemIdSet);
                        }
                        int maxTaskCount = 5;
                        int usedTaskCount = 0;
                        int index = 1;
                        List<string> itemIdList = new List<string>();
                        object usedTaskLockObj = new object();
                        foreach (string itemId in itemList)
                        {
                            itemIdList.Add(itemId);
                            if (index % 20 == 0 || index >= itemList.Count)
                            {
                                while (usedTaskCount >= maxTaskCount)
                                {
                                    Thread.Sleep(1000);
                                }
                                usedTaskCount++;
                                List<string> tempList = new List<string>(itemIdList);
                                itemIdList.Clear();
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        if (tempList != null)
                                        {
                                            foreach (string tempItemId in tempList)
                                            {
                                                SaveItemDetail(tempItemId, false);
                                                lock (shopItemIdSetLockObj)
                                                {
                                                    shopItemIdSet.Remove(tempItemId);
                                                }
                                            }
                                            lock (shopItemIdSetLockObj)
                                            {
                                                //保存
                                                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopItemSaveFileName), string.Join(Environment.NewLine, new List<string>(shopItemIdSet)));
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    finally
                                    {
                                        lock (usedTaskLockObj)
                                        {
                                            usedTaskCount--;
                                        }
                                    }
                                });
                            }
                            index++;
                            //SaveItemDetail(itemId);
                            //itemIdSet.Remove(itemId);
                            //Thread.Sleep(20);
                        }
                        while (usedTaskCount > 0)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchShopItemDetail Error", ex);
                }
            }
        }

        private void SaveItemDetail(string itemId, bool isAddShop)
        {
            try
            {
                string itemStr = MoguUtil.GetItemData(itemId);
                if (!string.IsNullOrWhiteSpace(itemStr) && itemStr.Contains("商品不存在"))
                {
                    return;
                }
                //Console.WriteLine($"{itemId} -> {itemStr}");
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "items");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(Path.Combine(dir, $"{itemId}.txt"), itemStr);
                var match = Regex.Match(itemStr, @"(?is)""shopId""\s*:\s*""(?<shopId>[^""]*)""");
                if (match.Success)
                {
                    var shopId = match.Groups["shopId"].Value;
                    if (!shopIdSet.Contains(shopId))
                    {
                        shopIdSet.Add(shopId);
                        //保存
                        FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName), string.Join(Environment.NewLine, new List<string>(shopIdSet)));
                    }
                    var dir2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shopItems", shopId, "details");
                    if (!Directory.Exists(dir2))
                    {
                        Directory.CreateDirectory(dir2);
                    }
                    FileUtil.Write(Path.Combine(dir2, $"{itemId}.txt"), itemStr);
                }

                //HttpDetailApiResponse itemDetailResponse = JsonConvert.DeserializeObject<HttpDetailApiResponse>(itemStr);
                //if (itemDetailResponse != null && itemDetailResponse.Data != null && itemDetailResponse.Data.Result != null)
                //{
                //    BaseDao baseDao = new BaseDao();
                //    ShopInfo shopInfo = itemDetailResponse.Data.Result.ShopInfo;
                //    if (shopInfo != null)
                //    {
                //        MoguShop moguShop = null;
                //        IList<MoguShop> moguShopList = baseDao.Get(new MoguShop() { ShopId = shopInfo.ShopId });
                //        if (moguShopList == null || moguShopList.Count == 0)
                //        {
                //            moguShop = new MoguShop()
                //            {
                //                CreateTime = DateTime.Now
                //            };
                //            moguShop.ShopId = shopInfo.ShopId;
                //            moguShop.Name = shopInfo.Name;
                //            moguShop.ShopUrl = shopInfo.ShopUrl;
                //            moguShop.ShopLogo = shopInfo.ShopLogo;
                //            baseDao.Add(moguShop);
                //        }
                //        else
                //        {
                //            moguShop = moguShopList.FirstOrDefault();
                //            if (moguShop.ModifyTime < DateTime.Now.AddDays(-7) || moguShop.CreateTime < DateTime.Now.AddDays(-7))
                //            {
                //                moguShop.ModifyTime = DateTime.Now;
                //                moguShop.ShopId = shopInfo.ShopId;
                //                moguShop.Name = shopInfo.Name;
                //                moguShop.ShopUrl = shopInfo.ShopUrl;
                //                moguShop.ShopLogo = shopInfo.ShopLogo;
                //                moguShopList.RemoveAt(0);
                //                baseDao.Delete(moguShopList);
                //                baseDao.Update(moguShop);
                //            }
                //        }

                //        ItemInfo itemInfo = itemDetailResponse.Data.Result.ItemInfo;
                //        if (itemInfo != null)
                //        {
                //            MoguItem moguItem = null;
                //            IList<MoguItem> moguItemList = baseDao.Get(new MoguItem() { ItemId = itemInfo.ItemId });
                //            if (moguItemList == null || moguItemList.Count == 0)
                //            {
                //                moguItem = new MoguItem()
                //                {
                //                    CreateTime = DateTime.Now
                //                };
                //                moguItem.ShopId = shopInfo.ShopId;
                //                moguItem.ItemId = itemInfo.ItemId;
                //                moguItem.Title = itemInfo.Title;
                //                moguItem.Desc = itemInfo.Desc;
                //                moguItem.LowPrice = Convert.ToDecimal(itemInfo.LowPrice);
                //                moguItem.LowNowPrice = Convert.ToDecimal(itemInfo.LowNowPrice);
                //                moguItem.HighPrice = Convert.ToDecimal(itemInfo.HighPrice);
                //                moguItem.HighNowPrice = Convert.ToDecimal(itemInfo.HighNowPrice);
                //                baseDao.Add(moguItem);
                //            }
                //            else
                //            {
                //                moguItem = moguItemList.FirstOrDefault();
                //                if (moguShop.ModifyTime >= DateTime.Now.AddDays(-7) || moguShop.CreateTime >= DateTime.Now.AddDays(-7))
                //                {
                //                    return;
                //                }
                //                moguItem.ModifyTime = DateTime.Now;
                //                moguItemList.RemoveAt(0);
                //                baseDao.Delete(moguItemList);
                //                baseDao.Update(moguItem);
                //            }

                //            //主图
                //            List<string> topImages = itemDetailResponse.Data.Result.TopImages;
                //            if (topImages != null)
                //            {
                //                IList<MoguItemTopImage> moguItemTopImages = baseDao.Get(new MoguItemTopImage() { ItemId = itemInfo.ItemId });
                //                if (moguItemTopImages == null)
                //                {
                //                    moguItemTopImages = new List<MoguItemTopImage>();
                //                    foreach (var imageUrl in topImages)
                //                    {
                //                        moguItemTopImages.Add(new MoguItemTopImage()
                //                        {
                //                            ItemId = itemInfo.ItemId,
                //                            ImageUrl = imageUrl,
                //                            CreateTime = DateTime.Now
                //                        });
                //                    }
                //                    baseDao.Add(moguItemTopImages);
                //                }
                //                else
                //                {
                //                    Dictionary<string, MoguItemTopImage> existImageUrl2TopImageObj = new Dictionary<string, MoguItemTopImage>();
                //                    List<MoguItemTopImage> removeTopImages = new List<MoguItemTopImage>();
                //                    foreach (MoguItemTopImage moguItemTopImage in moguItemTopImages)
                //                    {
                //                        if (existImageUrl2TopImageObj.ContainsKey(moguItemTopImage.ImageUrl))
                //                        {
                //                            removeTopImages.Add(moguItemTopImage);
                //                        }
                //                        else
                //                        {
                //                            existImageUrl2TopImageObj[moguItemTopImage.ImageUrl] = moguItemTopImage;
                //                        }
                //                    }
                //                    if (removeTopImages.Count > 0)
                //                    {
                //                        baseDao.Delete(removeTopImages);
                //                    }

                //                    List<MoguItemTopImage> topImageAddList = new List<MoguItemTopImage>();
                //                    List<MoguItemTopImage> topImageSaveList = new List<MoguItemTopImage>();
                //                    foreach (var imageUrl in topImages)
                //                    {
                //                        if (existImageUrl2TopImageObj.ContainsKey(imageUrl))
                //                        {
                //                            existImageUrl2TopImageObj[imageUrl].ModifyTime = DateTime.Now;
                //                            topImageSaveList.Add(existImageUrl2TopImageObj[imageUrl]);
                //                            existImageUrl2TopImageObj.Remove(imageUrl);
                //                        }
                //                        else
                //                        {
                //                            topImageAddList.Add(new MoguItemTopImage()
                //                            {
                //                                ItemId = itemInfo.ItemId,
                //                                ImageUrl = imageUrl,
                //                                CreateTime = DateTime.Now
                //                            });
                //                        }
                //                    }
                //                    if (topImageAddList.Count > 0)
                //                    {
                //                        baseDao.Add(topImageAddList);
                //                    }
                //                    if (topImageSaveList.Count > 0)
                //                    {
                //                        baseDao.Update(topImageSaveList);
                //                    }
                //                    if (existImageUrl2TopImageObj.Keys.Count > 0)
                //                    {
                //                        baseDao.Delete(existImageUrl2TopImageObj.Values.ToList());
                //                    }
                //                }
                //            }
                //            //详情图
                //            DetailInfo detailInfo = itemDetailResponse.Data.Result.DetailInfo;
                //            if (detailInfo != null)
                //            {
                //                IList<MoguItemDetailImage> moguItemDetailImages = baseDao.Get(new MoguItemDetailImage() { ItemId = itemInfo.ItemId });
                //                if (moguItemDetailImages == null)
                //                {
                //                    moguItemDetailImages = new List<MoguItemDetailImage>();
                //                    foreach (var detailImage in detailInfo.DetailImage)
                //                    {
                //                        foreach (var imageUrl in detailImage.List)
                //                        {
                //                            moguItemDetailImages.Add(new MoguItemDetailImage()
                //                            {
                //                                ItemId = itemInfo.ItemId,
                //                                ImageUrl = imageUrl,
                //                                CreateTime = DateTime.Now
                //                            });
                //                        }
                //                    }
                //                    baseDao.Add(moguItemDetailImages);
                //                }
                //                else
                //                {
                //                    Dictionary<string, MoguItemDetailImage> existImageUrl2DetailImageObj = new Dictionary<string, MoguItemDetailImage>();
                //                    List<MoguItemDetailImage> removeDetailImages = new List<MoguItemDetailImage>();
                //                    foreach (MoguItemDetailImage moguItemDetailImage in moguItemDetailImages)
                //                    {
                //                        if (existImageUrl2DetailImageObj.ContainsKey(moguItemDetailImage.ImageUrl))
                //                        {
                //                            removeDetailImages.Add(moguItemDetailImage);
                //                        }
                //                        else
                //                        {
                //                            existImageUrl2DetailImageObj[moguItemDetailImage.ImageUrl] = moguItemDetailImage;
                //                        }
                //                    }
                //                    if (removeDetailImages.Count > 0)
                //                    {
                //                        baseDao.Delete(removeDetailImages);
                //                    }

                //                    List<MoguItemDetailImage> detailImageAddList = new List<MoguItemDetailImage>();
                //                    List<MoguItemDetailImage> detailImageSaveList = new List<MoguItemDetailImage>();
                //                    foreach (var imageUrl in topImages)
                //                    {
                //                        if (existImageUrl2DetailImageObj.ContainsKey(imageUrl))
                //                        {
                //                            existImageUrl2DetailImageObj[imageUrl].ModifyTime = DateTime.Now;
                //                            detailImageAddList.Add(existImageUrl2DetailImageObj[imageUrl]);
                //                            existImageUrl2DetailImageObj.Remove(imageUrl);
                //                        }
                //                        else
                //                        {
                //                            detailImageAddList.Add(new MoguItemDetailImage()
                //                            {
                //                                ItemId = itemInfo.ItemId,
                //                                ImageUrl = imageUrl,
                //                                CreateTime = DateTime.Now
                //                            });
                //                        }
                //                    }
                //                    if (detailImageAddList.Count > 0)
                //                    {
                //                        baseDao.Add(detailImageAddList);
                //                    }
                //                    if (detailImageSaveList.Count > 0)
                //                    {
                //                        baseDao.Update(detailImageSaveList);
                //                    }
                //                    if (existImageUrl2DetailImageObj.Keys.Count > 0)
                //                    {
                //                        baseDao.Delete(existImageUrl2DetailImageObj.Values.ToList());
                //                    }
                //                }
                //            }
                //            //商品属性
                //            ItemParams itemParams = itemDetailResponse.Data.Result.ItemParams;
                //            if (itemParams != null)
                //            {
                //                if (itemParams.Info != null && itemParams.Info.Set != null)
                //                {
                //                    IList<MoguItemProp> moguItemProps = baseDao.Get(new MoguItemProp() { ItemId = itemInfo.ItemId });
                //                    if (moguItemProps == null)
                //                    {
                //                        moguItemProps = new List<MoguItemProp>();
                //                        foreach (var propName in itemParams.Info.Set.Keys)
                //                        {
                //                            moguItemProps.Add(new MoguItemProp()
                //                            {
                //                                ItemId = itemInfo.ItemId,
                //                                PropName = propName,
                //                                PropValue = itemParams.Info.Set[propName],
                //                                CreateTime = DateTime.Now
                //                            });
                //                        }
                //                        baseDao.Add(moguItemProps);
                //                    }
                //                    else
                //                    {
                //                        Dictionary<string, MoguItemProp> existPropName2PropObj = new Dictionary<string, MoguItemProp>();
                //                        List<MoguItemProp> removeProps = new List<MoguItemProp>();
                //                        foreach (MoguItemProp moguItemProp in moguItemProps)
                //                        {
                //                            if (existPropName2PropObj.ContainsKey(moguItemProp.PropName))
                //                            {
                //                                removeProps.Add(moguItemProp);
                //                            }
                //                            else
                //                            {
                //                                existPropName2PropObj[moguItemProp.PropName] = moguItemProp;
                //                            }
                //                        }
                //                        if (removeProps.Count > 0)
                //                        {
                //                            baseDao.Delete(removeProps);
                //                        }
                //                        List<MoguItemProp> propAddList = new List<MoguItemProp>();
                //                        List<MoguItemProp> propSaveList = new List<MoguItemProp>();
                //                        foreach (var propName in itemParams.Info.Set.Keys)
                //                        {
                //                            if (existPropName2PropObj.ContainsKey(propName))
                //                            {
                //                                existPropName2PropObj[propName].ModifyTime = DateTime.Now;
                //                                propAddList.Add(existPropName2PropObj[propName]);
                //                                existPropName2PropObj.Remove(propName);
                //                            }
                //                            else
                //                            {
                //                                propAddList.Add(new MoguItemProp()
                //                                {
                //                                    ItemId = itemInfo.ItemId,
                //                                    PropName = propName,
                //                                    PropValue = itemParams.Info.Set[propName],
                //                                    CreateTime = DateTime.Now
                //                                });
                //                            }
                //                        }
                //                        if (propAddList.Count > 0)
                //                        {
                //                            baseDao.Add(propAddList);
                //                        }
                //                        if (propSaveList.Count > 0)
                //                        {
                //                            baseDao.Update(propSaveList);
                //                        }
                //                        if (existPropName2PropObj.Keys.Count > 0)
                //                        {
                //                            baseDao.Delete(existPropName2PropObj.Values.ToList());
                //                        }
                //                    }
                //                }
                //            }
                //            //商品SKU属性
                //            SkuInfo skuInfo = itemDetailResponse.Data.Result.SkuInfo;
                //            if (skuInfo != null && skuInfo.Skus != null && skuInfo.Skus.Count > 0)
                //            {
                //                IList<MoguItemSku> moguItemSkus = baseDao.Get(new MoguItemSku() { ItemId = itemInfo.ItemId });
                //                if (moguItemSkus == null)
                //                {
                //                    moguItemSkus = new List<MoguItemSku>();
                //                    foreach (var sku in skuInfo.Skus)
                //                    {
                //                        moguItemSkus.Add(new MoguItemSku()
                //                        {
                //                            ItemId = itemInfo.ItemId,
                //                            SkuId = sku.XdSkuId,
                //                            NowPrice = sku.NowPrice,
                //                            Price = sku.Price,
                //                            Currency = sku.Currency,
                //                            Color = sku.Color,
                //                            StyleId = sku.StyleId,
                //                            SizeId = sku.SizeId,
                //                            Size = sku.Size,
                //                            Stock = sku.Stock,
                //                            Img = sku.Img,
                //                            CreateTime = DateTime.Now
                //                        });
                //                    }
                //                    baseDao.Add(moguItemSkus);
                //                }
                //                else
                //                {
                //                    Dictionary<string, MoguItemSku> existSkuId2SkuObj = new Dictionary<string, MoguItemSku>();
                //                    List<MoguItemSku> removeSkus = new List<MoguItemSku>();
                //                    foreach (MoguItemSku moguItemSku in removeSkus)
                //                    {
                //                        if (existSkuId2SkuObj.ContainsKey(moguItemSku.SkuId))
                //                        {
                //                            removeSkus.Add(moguItemSku);
                //                        }
                //                        else
                //                        {
                //                            existSkuId2SkuObj[moguItemSku.SkuId] = moguItemSku;
                //                        }
                //                    }
                //                    if (removeSkus.Count > 0)
                //                    {
                //                        baseDao.Delete(removeSkus);
                //                    }

                //                    List<MoguItemSku> skuAddList = new List<MoguItemSku>();
                //                    List<MoguItemSku> skuSaveList = new List<MoguItemSku>();
                //                    foreach (var sku in skuInfo.Skus)
                //                    {
                //                        if (existSkuId2SkuObj.ContainsKey(sku.XdSkuId))
                //                        {
                //                            existSkuId2SkuObj[sku.XdSkuId].ModifyTime = DateTime.Now;
                //                            skuSaveList.Add(existSkuId2SkuObj[sku.XdSkuId]);
                //                            existSkuId2SkuObj.Remove(sku.XdSkuId);
                //                        }
                //                        else
                //                        {
                //                            skuAddList.Add(new MoguItemSku()
                //                            {
                //                                ItemId = itemInfo.ItemId,
                //                                SkuId = sku.XdSkuId,
                //                                NowPrice = sku.NowPrice,
                //                                Price = sku.Price,
                //                                Currency = sku.Currency,
                //                                Color = sku.Color,
                //                                StyleId = sku.StyleId,
                //                                SizeId = sku.SizeId,
                //                                Size = sku.Size,
                //                                Stock = sku.Stock,
                //                                Img = sku.Img,
                //                                CreateTime = DateTime.Now
                //                            });
                //                        }
                //                    }
                //                    if (skuAddList.Count > 0)
                //                    {
                //                        baseDao.Add(skuAddList);
                //                    }
                //                    if (skuSaveList.Count > 0)
                //                    {
                //                        baseDao.Update(skuSaveList);
                //                    }
                //                    if (existSkuId2SkuObj.Keys.Count > 0)
                //                    {
                //                        baseDao.Delete(existSkuId2SkuObj.Values.ToList());
                //                    }
                //                }
                //            }
                //        }

                //        if (isAddShop)
                //        {
                //            //是否需要抓取店铺所有商品
                //            if (IsNeedSnatchAllGoods(shopInfo.ShopId))
                //            {
                //                shopIdSet.Add(shopInfo.ShopId);
                //                //保存
                //                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName), string.Join(Environment.NewLine, new List<string>(shopIdSet)));
                //            }
                //            else
                //            {
                //                shopIdSet.Remove(shopInfo.ShopId);
                //                //保存
                //                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName), string.Join(Environment.NewLine, new List<string>(shopIdSet)));
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    Log.Write("SnatchItemDetail Data: itemId:" + itemId + ",itemDetailData:" + itemStr);
                //}
            }
            catch (Exception ex)
            {
                Log.Write("SaveItemDetail Error", ex);
            }
        }

        private void SnatchShopItem()
        {
            while (true)
            {
                try
                {
                    if (shopIdSet.Count > 0)
                    {
                        List<string> shopList = null;
                        lock (shopIdSetLockObj)
                        {
                            shopList = new List<string>(shopIdSet);
                        }
                        foreach (string shopId in shopList)
                        {
                            if (!IsNeedSnatchAllGoods(shopId))
                            {
                                lock (shopIdSetLockObj)
                                {
                                    shopIdSet.Remove(shopId);
                                }
                                continue;
                            }
                            int pageNo = 1;
                            int pageSize = 60;
                            int total = 0;
                            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shopItems", shopId);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            do
                            {
                                string shopStr = MoguUtil.GetShopData(shopId, pageNo, pageSize);
                                ShopGoodsAllResponse shopGoodsAllResponse = null;
                                try
                                {
                                    shopGoodsAllResponse = JsonConvert.DeserializeObject<ShopGoodsAllResponse>(shopStr);
                                    if (shopGoodsAllResponse != null && shopGoodsAllResponse.Data != null)
                                    {
                                        if (shopGoodsAllResponse.Data.List != null)
                                        {
                                            foreach (var goods in shopGoodsAllResponse.Data.List)
                                            {
                                                if (!string.IsNullOrEmpty(goods.Iid))
                                                {
                                                    shopItemIdSet.Add(goods.Iid);
                                                    if (!Directory.Exists(Path.Combine(dir, "ids")))
                                                    {
                                                        Directory.CreateDirectory(Path.Combine(dir, "ids"));
                                                    }
                                                    File.WriteAllText(Path.Combine(dir, "ids", $"{goods.Iid}.txt"), "");
                                                }
                                            }
                                        }
                                        total = shopGoodsAllResponse.Data.Total;
                                        if (pageNo == 1)
                                        {
                                            File.WriteAllText(Path.Combine(dir, $"total.txt"), total.ToString());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write("解析获取店铺所有商品数据出错:", ex);
                                }
                            } while (pageNo++ * pageSize < total);

                            lock (shopIdSetLockObj)
                            {
                                shopIdSet.Remove(shopId);
                            }
                            lock (shopItemIdSetLockObj)
                            {
                                //保存
                                FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopItemSaveFileName), string.Join(Environment.NewLine, new List<string>(shopItemIdSet)));
                            }
                            Thread.Sleep(20);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchShopItem Error", ex);
                }
            }
        }

        private bool IsNeedSnatchAllGoods(string shopId)
        {
            //MoguItemDao moguItemDao = new MoguItemDao();
            //int shopAllGoodsCount = moguItemDao.GetAllGoodsCount(shopId);
            //string shopStr = MoguUtil.GetShopData(shopId, 1, 60);
            //ShopGoodsAllResponse shopGoodsAllResponse = null;
            //int total = 0;
            //try
            //{
            //    shopGoodsAllResponse = JsonConvert.DeserializeObject<ShopGoodsAllResponse>(shopStr);
            //    if (shopGoodsAllResponse != null && shopGoodsAllResponse.Data != null)
            //    {
            //        total = shopGoodsAllResponse.Data.Total;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            //if (total <= shopAllGoodsCount)
            //{
            //    return false;
            //}
            //return true;
            return true;
        }

    }
}
