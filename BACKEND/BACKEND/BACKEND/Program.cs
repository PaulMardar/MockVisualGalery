

using BACKEND.DOMAIN;
using BACKEND.REPOSITORY;

namespace BACKEND
{
    public class Program
    {
        public static void Main(string[] args)
        {

            IRepository<User> userRepo = new MemoryRepository<User>();

            IRepository<Photo> photoRepo = new MemoryRepository<Photo>();

            // Read the file's bytes once and store them directly on the entity
            byte[] fileBytes = { 0xAA, 0xAA, 0xAA, 0xAA }; // stand-in for File.ReadAllBytes("sunset.jpg")

            var photo = new Photo(0, "sunset.jpg", (1920, 1080), new List<string> { "sunset", "nature" })
            {
                Content = fileBytes
            };

            photoRepo.Add(photo);

            var fetched = photoRepo.GetById(photo.Id);
            Console.WriteLine($"Stored photo: Id={fetched?.Id}, Name={fetched?.Name}.{fetched?.Extension}, Bytes={fetched?.Content.Length}");

        }





    }
}