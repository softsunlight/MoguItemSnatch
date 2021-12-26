using MoguItemSnatch.Model;
using softsunlight.orm;
using System;

namespace MoguItemSnatch.Dao
{
    public class MoguShopDao : BaseDao
    {
        public bool Exist(string shopId)
        {
            int count = Convert.ToInt32(client.ExecuteScalar("select count(0) from mogushop where shopId={0};", shopId));
            return count > 0;
        }
    }
}
