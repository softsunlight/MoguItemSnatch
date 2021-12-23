using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoguItemSnatch.Domain.Item
{
    /// <summary>
    /// 蘑菇街商品详情响应类
    /// </summary>
    public class HttpDetailApiResponse
    {
        /// <summary>
        /// Api
        /// </summary>
        [JsonProperty("api")]
        public string Api { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        [JsonProperty("v")]
        public string V { get; set; }
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty("ret")]
        public string Ret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("data")]
        public DetailData Data { get; set; }
        /// <summary>
        /// 是否需要cookie
        /// </summary>
        [JsonProperty("needHeaderCookie")]
        public bool NeedHeaderCookie { get; set; }
    }

    public class DetailData
    {
        [JsonProperty("result")]
        public DetailResult Result { get; set; }
        [JsonProperty("status")]
        public ResponseStatus Status { get; set; }
    }

    public class DetailResult
    {
        [JsonProperty("userInfo")]
        public UserInfo UserInfo { get; set; }
        [JsonProperty("shopInfo")]
        public ShopInfo ShopInfo { get; set; }
        [JsonProperty("itemInfo")]
        public ItemInfo ItemInfo { get; set; }
    }

    public class UserInfo
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
        [JsonProperty("isLogin")]
        public bool IsLogin { get; set; }
        [JsonProperty("admin")]
        public bool Admin { get; set; }
        [JsonProperty("isNewComer")]
        public bool IsNewComer { get; set; }
        [JsonProperty("shopId")]
        public string ShopId { get; set; }
        [JsonProperty("sellerId")]
        public string SellerId { get; set; }
    }

    public class ShopInfo
    {
        [JsonProperty("shopLogo")]
        public string ShopLogo { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("shopId")]
        public string ShopId { get; set; }
        [JsonProperty("shopUrl")]
        public string ShopUrl { get; set; }
    }

    public class ItemInfo
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
        [JsonProperty("detailInfo")]
        public DetailInfo DetailInfo { get; set; }
        [JsonProperty("topImages")]
        public List<string> TopImages { get; set; }
        [JsonProperty("itemParams")]
        public ItemParams ItemParams { get; set; }
        [JsonProperty("skuInfo")]
        public SkuInfo SkuInfo { get; set; }
    }

    public class DetailInfo
    {
        [JsonProperty("splitDetailImage")]
        public bool SplitDetailImage { get; set; }
        [JsonProperty("desc")]
        public string Desc { get; set; }
        [JsonProperty("detailImage")]
        public List<DetailImage> DetailImage { get; set; }
    }

    public class DetailImage
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("anchor")]
        public string Anchor { get; set; }
        [JsonProperty("list")]
        public List<string> List { get; set; }
    }

    public class ItemParams
    {
        [JsonProperty("info")]
        public ItemParamsInfo Info { get; set; }
    }

    public class ItemParamsInfo
    {
        [JsonProperty("set")]
        public Dictionary<string, string> Set { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("anchor")]
        public string Anchor { get; set; }
    }

    public class SkuInfo
    {
        [JsonProperty("sizeKey")]
        public string SizeKey { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("skus")]
        public List<Sku> Skus { get; set; }
        [JsonProperty("props")]
        public List<Prop> Props { get; set; }
        [JsonProperty("priceRange")]
        public string PriceRange { get; set; }
        [JsonProperty("defaultPrice")]
        public string DefaultPrice { get; set; }
        [JsonProperty("totalStock")]
        public int TotalStock { get; set; }
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
        [JsonProperty("img")]
        public string Img { get; set; }
        [JsonProperty("canInstallment")]
        public bool CanInstallment { get; set; }
    }

    public class Sku
    {
        [JsonProperty("nowprice")]
        public int NowPrice { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("color")]
        public string Color { get; set; }
        [JsonProperty("styleId")]
        public int StyleId { get; set; }
        [JsonProperty("delayReasonCode")]
        public string DelayReasonCode { get; set; }
        [JsonProperty("stockId")]
        public string StockId { get; set; }
        [JsonProperty("xdSkuId")]
        public string XdSkuId { get; set; }
        [JsonProperty("size")]
        public string Size { get; set; }
        [JsonProperty("price")]
        public int Price { get; set; }
        [JsonProperty("stock")]
        public int Stock { get; set; }
        [JsonProperty("style")]
        public string Style { get; set; }
        [JsonProperty("sizeId")]
        public int SizeId { get; set; }
        [JsonProperty("delayReason")]
        public string DelayReason { get; set; }
    }

    public class Prop
    {
        [JsonProperty("list")]
        public List<PropItem> List { get; set; }
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class PropItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("default")]
        public bool Default { get; set; }
        [JsonProperty("styleId")]
        public int StyleId { get; set; }
        [JsonProperty("sizeId")]
        public int SizeId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
    }

    public class ResponseStatus
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

}
