using BACKEND.DOMAIN.Objects;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES
{
    public class PhotoService : IPhotoService
    {
        private IRepository<Photo> _photoRepository;

        public PhotoService(IRepository<Photo> photoRepository)
        {
            _photoRepository = photoRepository;
        }

        public Photo Upload(string fileName, (int length, int width) dimensions, List<string> tags, byte[] content)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
                throw new ArgumentException("File name must include a name and extension, e.g. 'REVELION2025.jpg'.", nameof(fileName));
            if (content is null || content.Length == 0)
                throw new ArgumentException("Photo content cannot be empty.", nameof(content));
            var photo = new Photo(0, fileName, dimensions.length, dimensions.width, tags)  {Content = content };
            return Add(photo);
        }

        public Photo Add(Photo entity) => _photoRepository.Add(entity);

        public Photo? GetById(int id) => _photoRepository.GetById(id);

        public IEnumerable<Photo> GetAll() => _photoRepository.GetAll();

        public bool AddTag(int id, string tag)
        {
            var photo = _photoRepository.GetById(id);
            if (photo is null)
                return false;
            if (!string.IsNullOrWhiteSpace(tag) && !photo.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                photo.Tags.Add(tag);
                _photoRepository.Update(photo);
            }
            return true;
        }

        public bool RemoveTag(int id, string tag)
        {
            var photo = _photoRepository.GetById(id);
            if (photo is null)
                return false;
            var categorie = photo.Tags.FirstOrDefault(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
            if (categorie is null)
                return false;
            photo.Tags.Remove(categorie);
            _photoRepository.Update(photo);
            return true;
        }

        public bool Delete(int id) => _photoRepository.Delete(id);
    }
}
