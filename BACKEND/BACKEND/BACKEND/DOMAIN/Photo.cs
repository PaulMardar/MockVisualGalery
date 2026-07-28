using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace BACKEND.DOMAIN
{
    public class Photo : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public (int length, int width) Dimensions { get; set; } = (0, 0);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> Tags { get; set; } = new List<string>();
        public string Extension { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();

        public Photo() { }
        public Photo(int id, string name, (int length, int width) dimensions, List<string> tags)
        {
            Id = id;
            (Name, Extension) = (name.Split('.')[0], name.Split('.')[1]);
            Dimensions = dimensions;
            Tags = tags;
        }
    }
}

