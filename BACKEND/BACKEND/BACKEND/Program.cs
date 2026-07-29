

using BACKEND.DOMAIN;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES;
using BACKEND.SERVICES.Interfaces;

namespace BACKEND
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IRepository<User> userRepo = new MemoryRepository<User>();
            IUserService userService = new UserService(userRepo);

            IRepository<Photo> photoRepo = new MemoryRepository<Photo>();
            IPhotoService photoService = new PhotoService(photoRepo);

            // ---- Add users ----
            Console.WriteLine("=== Adding Users ===");
            var alice = userService.Register("Alice Nakamura", "alice@example.com", "hashed-pw-1");
            var bob = userService.Register("Bob Ferreira", "bob@example.com", "hashed-pw-2");
            var carol = userService.Register("Carol Ionescu", "carol@example.com", "hashed-pw-3");

            foreach (var user in userService.GetAll())
                Console.WriteLine($"  Id={user.Id}, Name={user.Name}, Email={user.Email}");


            // ---- Add photos ----
            Console.WriteLine("\n=== Adding Photos ===");
            byte[] sunsetBytes = { 0xAA, 0xAA, 0xAA, 0xAA }; // stand-in for File.ReadAllBytes("sunset.jpg")
            byte[] mountainBytes = { 0xBB, 0xBB, 0xBB };     // stand-in for File.ReadAllBytes("mountain.png")
            byte[] cityBytes = { 0xCC, 0xCC };               // stand-in for File.ReadAllBytes("city.jpg")

            var sunsetPhoto = photoService.Upload("sunset.jpg", (1920, 1080), new List<string> { "sunset", "nature" }, sunsetBytes);
            var mountainPhoto = photoService.Upload("mountain.png", (3840, 2160), new List<string> { "mountain", "nature" }, mountainBytes);
            var cityPhoto = photoService.Upload("city.jpg", (1280, 720), new List<string> { "city", "night" }, cityBytes);

            foreach (var p in photoService.GetAll())
                Console.WriteLine($"  Id={p.Id}, Name={p.Name}.{p.Extension}, Tags=[{string.Join(", ", p.Tags)}]");

            // ---- Add tags ----
            Console.WriteLine("\n=== Adding Tags ===");
            photoService.AddTag(sunsetPhoto.Id, "beach");
            photoService.AddTag(sunsetPhoto.Id, "vacation");
            photoService.AddTag(mountainPhoto.Id, "hiking");
            photoService.AddTag(cityPhoto.Id, "skyline");
            photoService.AddTag(cityPhoto.Id, "city"); // duplicate, should be ignored (case-insensitive)

            PrintPhotoTags(photoService, sunsetPhoto.Id, "sunset.jpg");
            PrintPhotoTags(photoService, mountainPhoto.Id, "mountain.png");
            PrintPhotoTags(photoService, cityPhoto.Id, "city.jpg");

            // ---- Remove tags ----
            Console.WriteLine("\n=== Removing Tags ===");
            bool removedVacation = photoService.RemoveTag(sunsetPhoto.Id, "vacation");
            bool removedHiking = photoService.RemoveTag(mountainPhoto.Id, "hiking");
            bool removedNonExistent = photoService.RemoveTag(cityPhoto.Id, "underwater"); // not present

            Console.WriteLine($"  Removed 'vacation' from sunset.jpg: {removedVacation}");
            Console.WriteLine($"  Removed 'hiking' from mountain.png: {removedHiking}");
            Console.WriteLine($"  Removed 'underwater' from city.jpg (not present): {removedNonExistent}");

            PrintPhotoTags(photoService, sunsetPhoto.Id, "sunset.jpg");
            PrintPhotoTags(photoService, mountainPhoto.Id, "mountain.png");
            PrintPhotoTags(photoService, cityPhoto.Id, "city.jpg");







        }
        private static void PrintPhotoTags(IPhotoService photoService, int photoId, string label)
        {
            var photo = photoService.GetById(photoId);
            Console.WriteLine($"  {label} tags: [{string.Join(", ", photo?.Tags ?? new List<string>())}]");
        }
    }
}