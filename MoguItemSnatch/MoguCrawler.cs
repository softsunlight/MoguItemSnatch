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
using MoguItemSnatch.Model;
using MoguItemSnatch.Dao;

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
        /// 店铺ID集合
        /// </summary>
        private HashSet<string> shopIdSet;
        /// <summary>
        /// 启动爬虫
        /// </summary>
        public void Start()
        {

            //先恢复已保存的数据
            LoadExistedItemId();

            LoadExistedShopId();

            Task.Run(() =>
            {
                //采集商品详情
                SnatchItemDetail();
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
                else
                {
                    itemIdSet = new HashSet<string>();
                }
            }
            else
            {
                itemIdSet = new HashSet<string>();
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
                    if (itemIdSet.Count > 0)
                    {
                        List<string> itemList = new List<string>(itemIdSet);
                        foreach (string itemId in itemList)
                        {
                            SaveItemDetail(itemId);
                            itemIdSet.Remove(itemId);
                            Thread.Sleep(20);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("SnatchItemDetail Error", ex);
                }
            }
        }

        private void SaveItemDetail(string itemId)
        {
            string itemStr = MoguUtil.GetItemData(itemId);
            HttpDetailApiResponse itemDetailResponse = JsonConvert.DeserializeObject<HttpDetailApiResponse>(itemStr);
            if (itemDetailResponse != null && itemDetailResponse.Data != null && itemDetailResponse.Data.Result != null)
            {
                BaseDao baseDao = new BaseDao();
                ShopInfo shopInfo = itemDetailResponse.Data.Result.ShopInfo;
                if (shopInfo != null)
                {
                    MoguShop moguShop = null;
                    IList<MoguShop> moguShopList = baseDao.Get(new MoguShop() { ShopId = shopInfo.ShopId });
                    if (moguShopList == null || moguShopList.Count == 0)
                    {
                        moguShop = new MoguShop()
                        {
                            CreateTime = DateTime.Now
                        };
                    }
                    else
                    {
                        moguShop = moguShopList.FirstOrDefault();
                        moguShop.ModifyTime = DateTime.Now;
                        moguShopList.RemoveAt(0);
                        baseDao.Delete(moguShopList);
                    }
                    moguShop.ShopId = shopInfo.ShopId;
                    moguShop.Name = shopInfo.Name;
                    moguShop.ShopUrl = shopInfo.ShopUrl;
                    moguShop.ShopLogo = shopInfo.ShopLogo;
                    if (moguShop.Id > 0)
                    {
                        baseDao.Update(moguShop);
                    }
                    else
                    {
                        baseDao.Add(moguShop);
                    }

                    ItemInfo itemInfo = itemDetailResponse.Data.Result.ItemInfo;
                    if (itemInfo != null)
                    {
                        MoguItem moguItem = null;
                        IList<MoguItem> moguItemList = baseDao.Get(new MoguItem() { ItemId = itemInfo.ItemId });
                        if (moguItemList == null || moguItemList.Count == 0)
                        {
                            moguItem = new MoguItem()
                            {
                                CreateTime = DateTime.Now
                            };
                        }
                        else
                        {
                            moguItem = moguItemList.FirstOrDefault();
                            moguItem.ModifyTime = DateTime.Now;
                            moguItemList.RemoveAt(0);
                            baseDao.Delete(moguItemList);
                        }
                        moguItem.ShopId = shopInfo.ShopId;
                        moguItem.ItemId = itemInfo.ItemId;
                        moguItem.Title = itemInfo.Title;
                        moguItem.Desc = itemInfo.Desc;
                        moguItem.LowPrice = Convert.ToDecimal(itemInfo.LowPrice);
                        moguItem.LowNowPrice = Convert.ToDecimal(itemInfo.LowNowPrice);
                        moguItem.HighPrice = Convert.ToDecimal(itemInfo.HighPrice);
                        moguItem.HighNowPrice = Convert.ToDecimal(itemInfo.HighNowPrice);
                        if (moguItem.Id > 0)
                        {
                            baseDao.Update(moguItem);
                        }
                        else
                        {
                            baseDao.Add(moguItem);
                        }

                        //主图
                        List<string> topImages = itemDetailResponse.Data.Result.TopImages;
                        if (topImages != null)
                        {
                            IList<MoguItemTopImage> moguItemTopImages = baseDao.Get(new MoguItemTopImage() { ItemId = itemInfo.ItemId });
                            if (moguItemTopImages == null)
                            {
                                moguItemTopImages = new List<MoguItemTopImage>();
                                foreach (var imageUrl in topImages)
                                {
                                    moguItemTopImages.Add(new MoguItemTopImage()
                                    {
                                        ItemId = itemInfo.ItemId,
                                        ImageUrl = imageUrl,
                                        CreateTime = DateTime.Now
                                    });
                                }
                                baseDao.Add(moguItemTopImages);
                            }
                            else
                            {
                                Dictionary<string, MoguItemTopImage> existImageUrl2TopImageObj = new Dictionary<string, MoguItemTopImage>();
                                List<MoguItemTopImage> removeTopImages = new List<MoguItemTopImage>();
                                foreach (MoguItemTopImage moguItemTopImage in moguItemTopImages)
                                {
                                    if (existImageUrl2TopImageObj.ContainsKey(moguItemTopImage.ImageUrl))
                                    {
                                        removeTopImages.Add(moguItemTopImage);
                                    }
                                    else
                                    {
                                        existImageUrl2TopImageObj[moguItemTopImage.ImageUrl] = moguItemTopImage;
                                    }
                                }
                                if (removeTopImages.Count > 0)
                                {
                                    baseDao.Delete(removeTopImages);
                                }

                                List<MoguItemTopImage> topImageAddList = new List<MoguItemTopImage>();
                                List<MoguItemTopImage> topImageSaveList = new List<MoguItemTopImage>();
                                foreach (var imageUrl in topImages)
                                {
                                    if (existImageUrl2TopImageObj.ContainsKey(imageUrl))
                                    {
                                        existImageUrl2TopImageObj[imageUrl].ModifyTime = DateTime.Now;
                                        topImageSaveList.Add(existImageUrl2TopImageObj[imageUrl]);
                                        existImageUrl2TopImageObj.Remove(imageUrl);
                                    }
                                    else
                                    {
                                        topImageAddList.Add(new MoguItemTopImage()
                                        {
                                            ItemId = itemInfo.ItemId,
                                            ImageUrl = imageUrl,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                                if (topImageAddList.Count > 0)
                                {
                                    baseDao.Add(topImageAddList);
                                }
                                if (topImageSaveList.Count > 0)
                                {
                                    baseDao.Update(topImageSaveList);
                                }
                                if (existImageUrl2TopImageObj.Keys.Count > 0)
                                {
                                    baseDao.Delete(existImageUrl2TopImageObj.Values.ToList());
                                }
                            }
                        }
                        //详情图
                        DetailInfo detailInfo = itemDetailResponse.Data.Result.DetailInfo;
                        if (detailInfo != null)
                        {
                            IList<MoguItemDetailImage> moguItemDetailImages = baseDao.Get(new MoguItemDetailImage() { ItemId = itemInfo.ItemId });
                            if (moguItemDetailImages == null)
                            {
                                moguItemDetailImages = new List<MoguItemDetailImage>();
                                foreach (var detailImage in detailInfo.DetailImage)
                                {
                                    foreach (var imageUrl in detailImage.List)
                                    {
                                        moguItemDetailImages.Add(new MoguItemDetailImage()
                                        {
                                            ItemId = itemInfo.ItemId,
                                            ImageUrl = imageUrl,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                                baseDao.Add(moguItemDetailImages);
                            }
                            else
                            {
                                Dictionary<string, MoguItemDetailImage> existImageUrl2DetailImageObj = new Dictionary<string, MoguItemDetailImage>();
                                List<MoguItemDetailImage> removeDetailImages = new List<MoguItemDetailImage>();
                                foreach (MoguItemDetailImage moguItemDetailImage in moguItemDetailImages)
                                {
                                    if (existImageUrl2DetailImageObj.ContainsKey(moguItemDetailImage.ImageUrl))
                                    {
                                        removeDetailImages.Add(moguItemDetailImage);
                                    }
                                    else
                                    {
                                        existImageUrl2DetailImageObj[moguItemDetailImage.ImageUrl] = moguItemDetailImage;
                                    }
                                }
                                if (removeDetailImages.Count > 0)
                                {
                                    baseDao.Delete(removeDetailImages);
                                }

                                List<MoguItemDetailImage> detailImageAddList = new List<MoguItemDetailImage>();
                                List<MoguItemDetailImage> detailImageSaveList = new List<MoguItemDetailImage>();
                                foreach (var imageUrl in topImages)
                                {
                                    if (existImageUrl2DetailImageObj.ContainsKey(imageUrl))
                                    {
                                        existImageUrl2DetailImageObj[imageUrl].ModifyTime = DateTime.Now;
                                        detailImageAddList.Add(existImageUrl2DetailImageObj[imageUrl]);
                                        existImageUrl2DetailImageObj.Remove(imageUrl);
                                    }
                                    else
                                    {
                                        detailImageAddList.Add(new MoguItemDetailImage()
                                        {
                                            ItemId = itemInfo.ItemId,
                                            ImageUrl = imageUrl,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                                if (detailImageAddList.Count > 0)
                                {
                                    baseDao.Add(detailImageAddList);
                                }
                                if (detailImageSaveList.Count > 0)
                                {
                                    baseDao.Update(detailImageSaveList);
                                }
                                if (existImageUrl2DetailImageObj.Keys.Count > 0)
                                {
                                    baseDao.Delete(existImageUrl2DetailImageObj.Values.ToList());
                                }
                            }
                        }
                        //商品属性
                        ItemParams itemParams = itemDetailResponse.Data.Result.ItemParams;
                        if (itemParams != null)
                        {
                            if (itemParams.Info != null && itemParams.Info.Set != null)
                            {
                                IList<MoguItemProp> moguItemProps = baseDao.Get(new MoguItemProp() { ItemId = itemInfo.ItemId });
                                if (moguItemProps == null)
                                {
                                    moguItemProps = new List<MoguItemProp>();
                                    foreach (var propName in itemParams.Info.Set.Keys)
                                    {
                                        moguItemProps.Add(new MoguItemProp()
                                        {
                                            ItemId = itemInfo.ItemId,
                                            PropName = propName,
                                            PropValue = itemParams.Info.Set[propName],
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                    baseDao.Add(moguItemProps);
                                }
                                else
                                {
                                    Dictionary<string, MoguItemProp> existPropName2PropObj = new Dictionary<string, MoguItemProp>();
                                    List<MoguItemProp> removeProps = new List<MoguItemProp>();
                                    foreach (MoguItemProp moguItemProp in moguItemProps)
                                    {
                                        if (existPropName2PropObj.ContainsKey(moguItemProp.PropName))
                                        {
                                            removeProps.Add(moguItemProp);
                                        }
                                        else
                                        {
                                            existPropName2PropObj[moguItemProp.PropName] = moguItemProp;
                                        }
                                    }
                                    if (removeProps.Count > 0)
                                    {
                                        baseDao.Delete(removeProps);
                                    }
                                    List<MoguItemProp> propAddList = new List<MoguItemProp>();
                                    List<MoguItemProp> propSaveList = new List<MoguItemProp>();
                                    foreach (var propName in itemParams.Info.Set.Keys)
                                    {
                                        if (existPropName2PropObj.ContainsKey(propName))
                                        {
                                            existPropName2PropObj[propName].ModifyTime = DateTime.Now;
                                            propAddList.Add(existPropName2PropObj[propName]);
                                            existPropName2PropObj.Remove(propName);
                                        }
                                        else
                                        {
                                            propAddList.Add(new MoguItemProp()
                                            {
                                                ItemId = itemInfo.ItemId,
                                                PropName = propName,
                                                PropValue = itemParams.Info.Set[propName],
                                                CreateTime = DateTime.Now
                                            });
                                        }
                                    }
                                    if (propAddList.Count > 0)
                                    {
                                        baseDao.Add(propAddList);
                                    }
                                    if (propSaveList.Count > 0)
                                    {
                                        baseDao.Update(propSaveList);
                                    }
                                    if (existPropName2PropObj.Keys.Count > 0)
                                    {
                                        baseDao.Delete(existPropName2PropObj.Values.ToList());
                                    }
                                }
                            }
                        }
                        //商品SKU属性
                        SkuInfo skuInfo = itemDetailResponse.Data.Result.SkuInfo;
                        if (skuInfo != null && skuInfo.Skus != null && skuInfo.Skus.Count > 0)
                        {
                            IList<MoguItemSku> moguItemSkus = baseDao.Get(new MoguItemSku() { ItemId = itemInfo.ItemId });
                            if (moguItemSkus == null)
                            {
                                moguItemSkus = new List<MoguItemSku>();
                                foreach (var sku in skuInfo.Skus)
                                {
                                    moguItemSkus.Add(new MoguItemSku()
                                    {
                                        ItemId = itemInfo.ItemId,
                                        SkuId = sku.XdSkuId,
                                        NowPrice = sku.NowPrice,
                                        Price = sku.Price,
                                        Currency = sku.Currency,
                                        Color = sku.Color,
                                        StyleId = sku.StyleId,
                                        SizeId = sku.SizeId,
                                        Size = sku.Size,
                                        Stock = sku.Stock,
                                        Img = sku.Img,
                                        CreateTime = DateTime.Now
                                    });
                                }
                                baseDao.Add(moguItemSkus);
                            }
                            else
                            {
                                Dictionary<string, MoguItemSku> existSkuId2SkuObj = new Dictionary<string, MoguItemSku>();
                                List<MoguItemSku> removeSkus = new List<MoguItemSku>();
                                foreach (MoguItemSku moguItemSku in removeSkus)
                                {
                                    if (existSkuId2SkuObj.ContainsKey(moguItemSku.SkuId))
                                    {
                                        removeSkus.Add(moguItemSku);
                                    }
                                    else
                                    {
                                        existSkuId2SkuObj[moguItemSku.SkuId] = moguItemSku;
                                    }
                                }
                                if (removeSkus.Count > 0)
                                {
                                    baseDao.Delete(removeSkus);
                                }

                                List<MoguItemSku> skuAddList = new List<MoguItemSku>();
                                List<MoguItemSku> skuSaveList = new List<MoguItemSku>();
                                foreach (var sku in skuInfo.Skus)
                                {
                                    if (existSkuId2SkuObj.ContainsKey(sku.XdSkuId))
                                    {
                                        existSkuId2SkuObj[sku.XdSkuId].ModifyTime = DateTime.Now;
                                        skuSaveList.Add(existSkuId2SkuObj[sku.XdSkuId]);
                                        existSkuId2SkuObj.Remove(sku.XdSkuId);
                                    }
                                    else
                                    {
                                        skuAddList.Add(new MoguItemSku()
                                        {
                                            ItemId = itemInfo.ItemId,
                                            SkuId = sku.XdSkuId,
                                            NowPrice = sku.NowPrice,
                                            Price = sku.Price,
                                            Currency = sku.Currency,
                                            Color = sku.Color,
                                            StyleId = sku.StyleId,
                                            SizeId = sku.SizeId,
                                            Size = sku.Size,
                                            Stock = sku.Stock,
                                            Img = sku.Img,
                                            CreateTime = DateTime.Now
                                        });
                                    }
                                }
                                if (skuAddList.Count > 0)
                                {
                                    baseDao.Add(skuAddList);
                                }
                                if (skuSaveList.Count > 0)
                                {
                                    baseDao.Update(skuSaveList);
                                }
                                if (existSkuId2SkuObj.Keys.Count > 0)
                                {
                                    baseDao.Delete(existSkuId2SkuObj.Values.ToList());
                                }
                            }
                        }
                    }

                    //是否需要抓取店铺所有商品
                    if (IsNeedSnatchAllGoods(shopInfo.ShopId))
                    {
                        shopIdSet.Add(shopInfo.ShopId);
                        //保存
                        FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName), string.Join(Environment.NewLine, new List<string>(shopIdSet)));
                    }
                    else
                    {
                        shopIdSet.Remove(shopInfo.ShopId);
                        //保存
                        FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, shopSaveFileName), string.Join(Environment.NewLine, new List<string>(shopIdSet)));
                    }
                }
            }
            else
            {
                Log.Write("SnatchItemDetail Data:" + itemStr);
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
                        List<string> shopList = new List<string>(shopIdSet);
                        foreach (string shopId in shopList)
                        {
                            int pageNo = 1;
                            int pageSize = 60;
                            int total = 0;
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
                                                    itemIdSet.Add(goods.Iid);
                                                }
                                            }
                                        }
                                        total = shopGoodsAllResponse.Data.Total;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.Write("解析获取店铺所有商品数据出错:", ex);
                                    Log.Write("商品数据:" + shopStr);
                                }
                            } while (pageNo++ * pageSize < total);
                            //保存
                            FileUtil.Write(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, itemSaveFileName), string.Join(Environment.NewLine, new List<string>(itemIdSet)));
                            Thread.Sleep(20);
                        }
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
            MoguItemDao moguItemDao = new MoguItemDao();
            int shopAllGoodsCount = moguItemDao.GetAllGoodsCount(shopId);
            string shopStr = MoguUtil.GetShopData(shopId, 1, 60);
            ShopGoodsAllResponse shopGoodsAllResponse = null;
            int total = 0;
            try
            {
                shopGoodsAllResponse = JsonConvert.DeserializeObject<ShopGoodsAllResponse>(shopStr);
                if (shopGoodsAllResponse != null && shopGoodsAllResponse.Data != null)
                {
                    total = shopGoodsAllResponse.Data.Total;
                }
            }
            catch (Exception ex)
            {

            }
            if (total <= shopAllGoodsCount)
            {
                return false;
            }
            return true;
        }

    }
}
