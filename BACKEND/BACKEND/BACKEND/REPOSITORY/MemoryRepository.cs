using BACKEND.DOMAIN;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.REPOSITORY
{
    public class MemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private Dictionary<int, T> _items = new();
        private int _nextId = 1;

        public T? GetById(int id)
        {
            return _items.TryGetValue(id, out var item) ? item : default;
        }

        public IEnumerable<T> GetAll()
        {
            return _items.Values.ToList();
        }

        public T Add(T entity)
        {
            if (entity.Id <= 0)
            {
                entity.Id = _nextId++;
            }
            else if (entity.Id >= _nextId)
            {
                _nextId = entity.Id + 1;
            }

            _items.Add(entity.Id, entity);
            return entity;
        }

        public bool Update(T entity)
        {
            if (!_items.ContainsKey(entity.Id))
                return false;

            _items[entity.Id] = entity;
            return true;
        }

        public bool Delete(int id)
        {
            return _items.Remove(id);
        }
    }
}



