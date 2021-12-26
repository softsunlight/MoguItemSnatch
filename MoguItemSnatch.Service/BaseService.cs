using MoguItemSnatch.Dao;
using System;
using System.Collections.Generic;

namespace MoguItemSnatch.Service
{
    public class BaseService
    {
        protected BaseDao baseDao;

        public BaseService()
        {
            baseDao = new BaseDao();
        }

        public void Add<T>(T entity)
        {
            baseDao.Add(entity);
        }

        public void Add<T>(IList<T> entities)
        {
            baseDao.Add(entities);
        }

        public void Update<T>(T entity)
        {
            baseDao.Update(entity);
        }

        public void Update<T>(IList<T> entities)
        {
            baseDao.Update(entities);
        }

        public void Delete<T>(T entity)
        {
            baseDao.Delete(entity);
        }

        public void Delete<T>(IList<T> entities)
        {
            baseDao.Delete(entities);
        }

        public IEnumerable<T> Get<T>(T entity)
        {
            return baseDao.Get(entity);
        }
    }
}
