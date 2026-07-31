// BACKEND.REPOSITORY / PhotoFileStorage.cs
namespace BACKEND.REPOSITORY
{
    public interface IPhotoFileStorage
    {
        void Save(string key, string extension, byte[] content);
        byte[]? Read(string key, string extension);
        bool Delete(string key, string extension);
    }

    public class PhotoFileStorage : IPhotoFileStorage
    {
        private readonly string _directory;

        public PhotoFileStorage(string directory)
        {
            _directory = directory;
            Directory.CreateDirectory(_directory);
        }

        private string GetPath(string key, string extension) =>
            Path.Combine(_directory, $"{key}.{extension.TrimStart('.')}");

        public void Save(string key, string extension, byte[] content) =>
            File.WriteAllBytes(GetPath(key, extension), content);

        public byte[]? Read(string key, string extension)
        {
            var path = GetPath(key, extension);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public bool Delete(string key, string extension)
        {
            var path = GetPath(key, extension);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }
    }
}