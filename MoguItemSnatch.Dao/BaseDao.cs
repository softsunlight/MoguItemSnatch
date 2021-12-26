using softsunlight.orm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MoguItemSnatch.Dao
{
    public class BaseDao
    {
        protected SoftSunlightSqlClient client;

        public BaseDao()
        {
            string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            client = new SoftSunlightSqlClient(connStr);
        }

        public void Add<T>(T entity)
        {
            client.Add(entity);
        }

        public void Add<T>(IList<T> entities)
        {
            client.Add(entities);
        }

        public void Update<T>(T entity)
        {
            client.Update(entity);
        }

        public void Update<T>(IList<T> entities)
        {
            client.Update(entities);
        }

        public void Delete<T>(T entity)
        {
            client.Delete(entity);
        }

        public void Delete<T>(IList<T> entities)
        {
            client.Delete(entities);
        }

        public IEnumerable<T> Get<T>(T entity)
        {
            return client.Get(entity);
        }

    }
}
