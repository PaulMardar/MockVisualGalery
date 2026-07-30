using BACKEND.DOMAIN.Objects;
using BACKEND.REPOSITORY;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BACKEND.REPOSITORY
{
    public interface IRepository<T> where T : IEntity
    {
        T? GetById(int id);
        IEnumerable<T> GetAll();
        T Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }

    public class InFileRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public InFileRepository() : this(Path.Combine(AppContext.BaseDirectory, "DATA"))
        {
        }

        public InFileRepository(string storageDirectory)
        {
            _filePath = GetFilePathFor(storageDirectory);

            EnsureStorageFileExists();
        }

        // Each entity type gets its own file, e.g. "Data/User.json", "Data/Photo.json".
        private static string GetFilePathFor(string directory)
        {
            Directory.CreateDirectory(directory);
            var fileName = $"{typeof(T).Name}.json";
            return Path.Combine(directory, fileName);
        }

        private void EnsureStorageFileExists()
        {
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public T? GetById(int id)
        {
            lock (_lock)
            {
                return ReadAll().FirstOrDefault(e => e.Id == id);
            }
        }

        public IEnumerable<T> GetAll()
        {
            lock (_lock)
            {
                return ReadAll();
            }
        }

        public T Add(T entity)
        {
            lock (_lock)
            {
                var items = ReadAll();
                int nextId = items.Count == 0 ? 1 : items.Max(e => e.Id) + 1;

                if (entity.Id <= 0)
                {
                    entity.Id = nextId;
                }
                else if (items.Any(e => e.Id == entity.Id))
                {
                    throw new InvalidOperationException($"An entity with id '{entity.Id}' already exists.");
                }

                items.Add(entity);
                WriteAll(items);
                return entity;
            }
        }

        public bool Update(T entity)
        {
            lock (_lock)
            {
                var items = ReadAll();
                var index = items.FindIndex(e => e.Id == entity.Id);
                if (index < 0)
                    return false;

                items[index] = entity;
                WriteAll(items);
                return true;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                var items = ReadAll();
                var removed = items.RemoveAll(e => e.Id == id);
                if (removed == 0)
                    return false;

                WriteAll(items);
                return true;
            }
        }

        private List<T> ReadAll()
        {
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }

        private void WriteAll(List<T> items)
        {
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
