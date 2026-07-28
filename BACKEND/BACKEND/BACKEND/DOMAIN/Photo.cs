using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.DOMAIN
{
    public class Photo : IEntity
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;
        public (int length, int width) Dimensions { get; set; } = (0, 0);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
