using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace BACKEND.DOMAIN.Objects
{
    public class Photo : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dimensions Dimensions { get; set; } = new Dimensions();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> Tags { get; set; } = new List<string>();
        public string Extension { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public int OwnerId { get; set; } = -1;
        public Photo() { }
        public Photo(int id, string name, int length, int width, List<string> tags)
        {
            Id = id;
            (Name, Extension) = (name.Split('.')[0], name.Split('.')[1]);
            Dimensions = new Dimensions { Length = length, Width = width };
            Tags = tags;
        }
    }
}

