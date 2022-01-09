using System;
using System.Collections.Generic;
using System.Text;

namespace MoguItemSnatch.Model
{
    public class MoguItem
    {
        public int Id { get; set; }
        public string ShopId { get; set; }
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public decimal LowPrice { get; set; }
        public decimal LowNowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal HighNowPrice { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }

    public class MoguItemTopImage
    {
        public int Id { get; set; }
        public string ItemId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }

    public class MoguItemDetailImage
    {
        public int Id { get; set; }
        public string ItemId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }

    public class MoguItemProp
    {
        public int Id { get; set; }
        public string ItemId { get; set; }
        public string PropName { get; set; }
        public string PropValue { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }

    public class MoguItemSku
    {
        public int Id { get; set; }
        public string ItemId { get; set; }
        public string SkuId { get; set; }
        public int NowPrice { get; set; }
        public int Price { get; set; }
        public string Currency { get; set; }
        public string Color { get; set; }
        public int StyleId { get; set; }
        public int SizeId { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }
        public string Img { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }

}
