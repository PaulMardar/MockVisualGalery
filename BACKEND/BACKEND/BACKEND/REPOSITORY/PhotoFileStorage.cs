// BACKEND.REPOSITORY / PhotoFileStorage.cs
namespace BACKEND.REPOSITORY
{
    public interface IPhotoFileStorage
    {
        void Save(int photoId, string extension, byte[] content);
        byte[]? Read(int photoId, string extension);
        bool Delete(int photoId, string extension);
    }

    public class PhotoFileStorage : IPhotoFileStorage
    {
        private readonly string _directory;

        public PhotoFileStorage(string directory)
        {
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        private string GetPath(int photoId, string extension) =>
            Path.Combine(_directory, $"{photoId}.{extension.TrimStart('.')}");

        public void Save(int photoId, string extension, byte[] content) =>
            File.WriteAllBytes(GetPath(photoId, extension), content);

        public byte[]? Read(int photoId, string extension)
        {
            var path = GetPath(photoId, extension);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public bool Delete(int photoId, string extension)
        {
            var path = GetPath(photoId, extension);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }
    }
}