using System;
using System.Collections.Generic;
using System.Text;

namespace MoguItemSnatch.Dao
{
    public class MoguItemDao : BaseDao
    {
        /// <summary>
        /// 获取店铺所有商品数量
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public int GetAllGoodsCount(string shopId)
        {
            int count = Convert.ToInt32(client.ExecuteScalar("select count(0) from moguitem where shopId={0};", shopId));
            return count;
        }
    }
}
