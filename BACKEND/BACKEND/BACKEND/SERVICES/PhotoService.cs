using BACKEND.DOMAIN.Objects;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND.SERVICES
{
    public interface IPhotoService : IService<Photo>
    {
        Photo Upload(string fileName, (int length, int width) dimensions, List<string> tags, byte[] content);
        bool AddTag(int id, string tag);
        bool RemoveTag(int id, string tag);
    }
    public class PhotoService : IPhotoService
    {
        private readonly IRepository<Photo> _photoRepository;
        private readonly IPhotoFileStorage _fileStorage;

        public PhotoService(IRepository<Photo> photoRepository, IPhotoFileStorage fileStorage)
        {
            _photoRepository = photoRepository;
            _fileStorage = fileStorage;
        }

        public Photo Upload(string fileName, (int length, int width) dimensions, List<string> tags, byte[] content)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
                throw new ArgumentException("File name must include a name and extension, e.g. 'REVELION2025.jpg'.", nameof(fileName));
            if (content is null || content.Length == 0)
                throw new ArgumentException("Photo content cannot be empty.", nameof(content));

            var photo = new Photo(0, fileName, dimensions.length, dimensions.width, tags);
            var added = _photoRepository.Add(photo);          // now has its real Id
            _fileStorage.Save(added.Id.ToString(), added.Extension, content);

            _ = GenerateVariantsAsync(added.Id, content);

            return added;
        }

        // Generates every filter in PhotoFilters.Variants in parallel and
        // saves each one (locally, and to S3 too if syncing is enabled)
        // under "{id}_{suffix}.png" - e.g. "7_inverted.png", "7_sepia.png",
        // "7_grayscale.png" (variants are always PNG regardless of the
        // original's format - see PhotoFilters.VariantExtension). A failure
        // on one filter (corrupt image, unsupported format, S3 hiccup) is
        // logged and does not affect the others or the original upload,
        // which has already completed by the time this runs.
        private async Task GenerateVariantsAsync(int photoId, byte[] originalContent)
        {
            var tasks = PhotoFilters.Variants.Select(variant => Task.Run(() =>
            {
                try
                {
                    var variantBytes = PhotoFilters.Apply(originalContent, variant.Apply);
                    _fileStorage.Save($"{photoId}_{variant.Suffix}", PhotoFilters.VariantExtension, variantBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Failed to generate '{variant.Suffix}' variant for photo {photoId}: {ex.Message}");
                }
            }));

            await Task.WhenAll(tasks);
        }

        public Photo Add(Photo entity)
        {
            var added = _photoRepository.Add(entity);
            if (entity.Content.Length > 0)
            {
                _fileStorage.Save(added.Id.ToString(), added.Extension, entity.Content);
                _ = GenerateVariantsAsync(added.Id, entity.Content);
            }
            return added;
        }

        public Photo? GetById(int id)
        {
            var photo = _photoRepository.GetById(id);
            if (photo is null) return null;
            photo.Content = _fileStorage.Read(id.ToString(), photo.Extension) ?? Array.Empty<byte>();
            return photo;
        }

        public IEnumerable<Photo> GetAll() => _photoRepository.GetAll(); // metadata only; call GetById for bytes

        public bool Delete(int id)
        {
            var photo = _photoRepository.GetById(id);
            if (photo is null) return false;

            _fileStorage.Delete(id.ToString(), photo.Extension);

            // Clean up every filtered variant too, so deleting a photo doesn't
            // leave "{id}_inverted.png" etc. orphaned on disk and in S3.
            foreach (var variant in PhotoFilters.Variants)
                _fileStorage.Delete($"{id}_{variant.Suffix}", PhotoFilters.VariantExtension);

            return _photoRepository.Delete(id);
        }


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
    }
}