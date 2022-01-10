using softsunlight.orm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace MoguItemSnatch.Dao
{
    /// <summary>
    /// 基础数据库访问类
    /// </summary>
    public class BaseDao
    {
        protected SoftSunlightSqlClient client;

        public BaseDao()
        {
            string connStr = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            client = new SoftSunlightSqlClient(connStr);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Add<T>(T entity)
        {
            client.Add(entity);
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Add<T>(IList<T> entities)
        {
            client.Add(entities);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(T entity)
        {
            client.Update(entity);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Update<T>(IList<T> entities)
        {
            client.Update(entities);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Delete<T>(T entity)
        {
            client.Delete(entity);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Delete<T>(IList<T> entities)
        {
            client.Delete(entities);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IList<T> Get<T>(T entity)
        {
            return client.Get(entity);
        }

    }
}
