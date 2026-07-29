using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.DOMAIN.DTOS
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Width { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
    }
    public class UploadPhotoRequest
    {
        public string FileName { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Width { get; set; }
        public List<string> Tags { get; set; } = new();
        public byte[] Content { get; set; } = Array.Empty<byte>();
    }
    public class TagRequest
    {
        public string Tag { get; set; } = string.Empty;
    }

    public class CreatePhotoRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Width { get; set; }
        public List<string> Tags { get; set; } = new();
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public int OwnerId { get; set; } = -1;
    }
}
