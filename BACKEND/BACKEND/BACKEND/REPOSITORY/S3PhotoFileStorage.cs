// BACKEND.REPOSITORY / S3PhotoFileStorage.cs
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using BACKEND.CONFIGURATION;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;

namespace BACKEND.REPOSITORY
{
    // Drop-in replacement for PhotoFileStorage that stores photo bytes in an
    // S3 bucket instead of on local disk. Same interface, same call sites -
    // PhotoService doesn't need to know which one is active.
    public class S3PhotoFileStorage : IPhotoFileStorage
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3PhotoFileStorage(IOptions<S3Options> options)
        {
            var s3Options = options.Value;

            if (string.IsNullOrWhiteSpace(s3Options.BucketName))
                throw new InvalidOperationException(
                    "S3:BucketName must be configured in appsettings.json or an environment variable before starting the app.");

            _bucketName = s3Options.BucketName;
            var region = RegionEndpoint.GetBySystemName(s3Options.Region);

            // Explicit keys if provided, otherwise fall back to the default
            // AWS credential chain (env vars, ~/.aws/credentials, or an IAM
            // role). Preferring the chain over hardcoded keys is the
            // recommended path - see the setup guide.
            _s3Client = !string.IsNullOrWhiteSpace(s3Options.AccessKey) && !string.IsNullOrWhiteSpace(s3Options.SecretKey)
                ? new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, region)
                : new AmazonS3Client(region);
        }

        private static string GetKey(string key, string extension) =>
            $"{key}.{extension.TrimStart('.')}";

        public void Save(string key, string extension, byte[] content)
        {
            using var stream = new MemoryStream(content);
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = GetKey(key, extension),
                InputStream = stream,
                AutoCloseStream = true
            };

            // Current AWSSDK.S3 only exposes the *Async methods - IPhotoFileStorage
            // is synchronous (to match PhotoFileStorage/PhotoService), so we block
            // on the async call here rather than making the sync/async mismatch all over the places

            _s3Client.PutObjectAsync(request).GetAwaiter().GetResult();
        }

        public byte[]? Read(string key, string extension)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetKey(key, extension)
                };

                using var response = _s3Client.GetObjectAsync(request).GetAwaiter().GetResult();
                using var ms = new MemoryStream();
                response.ResponseStream.CopyTo(ms);
                return ms.ToArray();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public bool Delete(string key, string extension)
        {
            var s3Key = GetKey(key, extension);

            try
            {
                _s3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key
                }).GetAwaiter().GetResult();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = s3Key
            }).GetAwaiter().GetResult();
            return true;
        }
    }
}