using System;

namespace MoguItemSnatch.Model
{
    /// <summary>
    /// 蘑菇街店铺
    /// </summary>
    public class MoguShop
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public string ShopId { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 店铺LOGO
        /// </summary>
        public string ShopLogo { get; set; }
        /// <summary>
        ///店铺地址
        /// </summary>
        public string ShopUrl { get; set; }
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
