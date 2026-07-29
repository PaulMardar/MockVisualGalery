using BACKEND.DOMAIN;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES
{
    public class PhotoService : IPhotoService

    {
        public Photo Add(Photo entity)
        {
            throw new NotImplementedException();
        }

        public bool AddTag(int id, string tag)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Photo> GetAll()
        {
            throw new NotImplementedException();
        }

        public Photo? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Photo Upload(string fileName, (int length, int width) dimensions, List<string> tags, byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
