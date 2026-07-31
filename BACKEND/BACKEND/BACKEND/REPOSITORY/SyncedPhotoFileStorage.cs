// BACKEND.REPOSITORY / SyncedPhotoFileStorage.cs
using System;

namespace BACKEND.REPOSITORY
{
    // Wraps two IPhotoFileStorage implementations: local disk always runs
    // and stays the source of truth for reads; the second one (typically
    // S3PhotoFileStorage) is optional and, when supplied, gets every write
    // and delete mirrored to it as well.
    public class SyncedPhotoFileStorage : IPhotoFileStorage
    {
        private readonly IPhotoFileStorage _local;
        private readonly IPhotoFileStorage? _s3;

        public SyncedPhotoFileStorage(IPhotoFileStorage local, IPhotoFileStorage? s3 = null)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
            _s3 = s3;
        }

        public void Save(string key, string extension, byte[] content)
        {
            _local.Save(key, extension, content);
            _s3?.Save(key, extension, content);
        }

        // Local disk is always the read path - S3 is a mirror, not a
        // fallback, so reads never depend on it being reachable.
        public byte[]? Read(string key, string extension) =>
            _local.Read(key, extension);

        public bool Delete(string key, string extension)
        {
            var localDeleted = _local.Delete(key, extension);
            _s3?.Delete(key, extension);
            return localDeleted;
        }
    }
}