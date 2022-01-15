using System;
using System.Collections.Generic;
using System.Text;

namespace MoguItemSnatch.Model
{
    /// <summary>
    /// 蘑菇街商品
    /// </summary>
    public class MoguItem
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 所属店铺ID
        /// </summary>
        public string ShopId { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// SKU最低价
        /// </summary>
        public string LowPrice { get; set; }
        /// <summary>
        /// 当前SKU最低价
        /// </summary>
        public string LowNowPrice { get; set; }
        /// <summary>
        /// SKU最高价
        /// </summary>
        public string HighPrice { get; set; }
        /// <summary>
        /// 当前SKU最高价
        /// </summary>
        public string HighNowPrice { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }

    /// <summary>
    /// 蘑菇街商品主图
    /// </summary>
    public class MoguItemTopImage
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }

    /// <summary>
    /// 蘑菇街商品详情图
    /// </summary>
    public class MoguItemDetailImage
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }

    /// <summary>
    /// 蘑菇街商品属性
    /// </summary>
    public class MoguItemProp
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropName { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        public string PropValue { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }

    /// <summary>
    /// 蘑菇街商品SKU
    /// </summary>
    public class MoguItemSku
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ItemId { get; set; }
        /// <summary>
        /// SKU ID
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        public int NowPrice { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }
        /// <summary>
        /// 价格单位(人民币)
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int StyleId { get; set; }
        /// <summary>
        /// 尺码ID
        /// </summary>
        public int SizeId { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }
        /// <summary>
        /// SKU 图片地址
        /// </summary>
        public string Img { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
    }

}
