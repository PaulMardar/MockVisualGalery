using BACKEND.DOMAIN;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.Interfaces
{
    public interface IService<T> where T : IEntity
    {
        T? GetById(int id);
        IEnumerable<T> GetAll();
        T Add(T entity);
        bool Delete(int id);
    }
}
