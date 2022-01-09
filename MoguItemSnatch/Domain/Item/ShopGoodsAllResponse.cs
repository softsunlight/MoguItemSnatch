using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoguItemSnatch.Domain.Item
{
    /// <summary>
    /// 蘑菇街店铺所有商品响应类
    /// </summary>
    public class ShopGoodsAllResponse
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
        public ShopGoodsAllData Data { get; set; }
        /// <summary>
        /// 是否需要cookie
        /// </summary>
        [JsonProperty("needHeaderCookie")]
        public bool NeedHeaderCookie { get; set; }
    }

    public class ShopGoodsAllData
    {
        [JsonProperty("list")]
        public List<ShopGoods> List { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("nextPage")]
        public int NextPage { get; set; }
        [JsonProperty("curPage")]
        public int CurPage { get; set; }
        [JsonProperty("page")]
        public int Page { get; set; }
    }

    public class ShopGoods
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        [JsonProperty("Iid")]
        public string Iid { get; set; }
    }

}
