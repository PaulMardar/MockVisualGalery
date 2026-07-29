using BACKEND.DOMAIN;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.Interfaces
{
    public interface IPhotoService : IService<Photo>
    {
        Photo Upload(string fileName, (int length, int width) dimensions, List<string> tags, byte[] content);
        bool AddTag(int id, string tag);
        bool RemoveTag(int id, string tag);
    }
}
