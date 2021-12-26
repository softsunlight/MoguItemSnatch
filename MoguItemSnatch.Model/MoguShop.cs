using System;

namespace MoguItemSnatch.Model
{
    public class MoguShop
    {
        public int Id { get; set; }
        public string ShopId { get; set; }
        public string Name { get; set; }
        public string ShopLogo { get; set; }
        public string ShopUrl { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
