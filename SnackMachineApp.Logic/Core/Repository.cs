﻿using SnackMachineApp.Logic.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnackMachineApp.Logic.Core
{
    public class Repository<T> : IRepository<T> where T : AggregateRoot
    {
        public T Get(long id)
        {
            using (var session = SessionFactory.OpenSession())
            {
                return session.Get<T>(id);
            }
        }

        public IList<T> List()
        {
            using (var session = SessionFactory.OpenSession())
            {
                return session.QueryOver<T>().List<T>();
            };
        }

        public void Save(T entity)
        {
            using (var session = SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdateAsync(entity);
                transaction.Commit();
            }
        }

        public void Delete(T entity)
        {
            using (var session = SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.Delete(entity);
                transaction.Commit();
            }
        }

    }
}
