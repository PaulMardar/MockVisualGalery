using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.CONFIGURATION
{
    // Bound from the "S3" section of appsettings.json.
    public class S3Options
    {
        // Master switch. false (default) = photos only ever touch local disk.
        // true = every save/delete on local disk is also mirrored to the bucket below.
        public bool Enabled { get; set; } = false;

        public string BucketName { get; set; } = string.Empty;
        public string Region { get; set; } = "eu-north-1";

        // Only set these if you're NOT relying on the default AWS credential
    
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
    }
}