using BACKEND.DOMAIN.Objects;
using BACKEND.REPOSITORY;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.REPOSITORY
{
    public class InFileRepository<T> : IRepository<T> where T : IEntity
    {
        public T Add(T entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public T? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public bool Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
