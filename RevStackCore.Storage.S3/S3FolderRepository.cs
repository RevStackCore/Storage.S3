using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using RevStackCore.Storage.Model;
using RevStackCore.Storage.Repository;

namespace RevStackCore.Storage.S3
{
    public class S3FolderRepository : IFolderRepository
    {
        private readonly S3DataContext _context;

        public S3FolderRepository(S3DataContext context)
        {
            _context = context;
        }

        public IFolder Add(string path)
        {
            path = Util.GetFilePath(path, true);

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                PutObjectRequest request = new PutObjectRequest();
                request.Key = path;
                request.BucketName = _context.Bucket;
                request.CannedACL = S3CannedACL.PublicRead;
                request.StorageClass = S3StorageClass.Standard;

                PutObjectResponse response = client.PutObjectAsync(request).Result;

            }

            return Get(path);
        }

        public void Delete(string path)
        {
            path = Util.GetFilePath(path, true);
            DeleteDirectory(path);
        }

        public IFolder Get(string path)
        {
            path = Util.GetFilePath(path, true);

            Folder folder = new Folder();
            folder.Path = path;
            folder.Size = this.RecurseForFoldersSize(path);
            folder.Url = _context.CDN + "/" + path;

            return folder;
        }

        public IFolder Update(string path, string destination)
        {
            path = Util.GetFilePath(path, true);
            destination = Util.GetFilePath(destination, true);

            if (path != destination)
            {
                MoveDirectory(path, destination);
                return Get(destination);
            }

            return Get(path);
        }

        #region "private"
        public void MoveDirectory(string sourceKey, string destinationKey)
        {

            //Find all keys with a prefex of sourceKey, and rename them with destinationKey for prefix
            try
            {
                using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
                {

                    DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest()
                    {
                        BucketName = _context.Bucket
                    };
                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest
                    {
                        BucketName = _context.Bucket,
                        Prefix = sourceKey,
                    };
                    do
                    {
                        ListObjectsResponse listObjectsResponse = client.ListObjectsAsync(listObjectsRequest).Result;
                        foreach (var s3Object in listObjectsResponse.S3Objects)
                        {
                            string newKey = s3Object.Key.Replace(sourceKey, destinationKey);

                            CopyObjectRequest copyObjectRequest = new CopyObjectRequest()
                            {
                                SourceBucket = _context.Bucket,
                                DestinationBucket = _context.Bucket,
                                SourceKey = s3Object.Key,
                                DestinationKey = newKey
                            };

                            CopyObjectResponse copyObectResponse = client.CopyObjectAsync(copyObjectRequest).Result;
                            deleteObjectsRequest.AddKey(s3Object.Key);
                        }
                        if (listObjectsResponse.IsTruncated)
                        {
                            listObjectsRequest.Marker = listObjectsResponse.NextMarker;
                        }
                        else
                        {
                            listObjectsRequest = null;
                        }

                    } while (listObjectsRequest != null);

                    client.DeleteObjectsAsync(deleteObjectsRequest);
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw amazonS3Exception;
            }
        }

        private void DeleteDirectory(string path)
        {

            //Find all keys with a prefex of sourceKey, and rename them with destinationKey for prefix
            try
            {
                using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
                {

                    DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest()
                    {
                        BucketName = _context.Bucket
                    };

                    ListObjectsRequest listObjectsRequest = new ListObjectsRequest
                    {
                        BucketName = _context.Bucket,
                        Prefix = path,
                    };

                    do
                    {
                        ListObjectsResponse listObjectsResponse = client.ListObjectsAsync(listObjectsRequest).Result;
                        foreach (var s3Object in listObjectsResponse.S3Objects)
                        {
                            deleteObjectsRequest.AddKey(s3Object.Key);
                        }
                        if (listObjectsResponse.IsTruncated)
                        {
                            listObjectsRequest.Marker = listObjectsResponse.NextMarker;
                        }
                        else
                        {
                            listObjectsRequest = null;
                        }

                    } while (listObjectsRequest != null);

                    client.DeleteObjectsAsync(deleteObjectsRequest);
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw amazonS3Exception;
            }
        }

        private long RecurseForFoldersSize(string path)
        {
            long total = 0;
            var folderList = new List<string>();

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = _context.Bucket;
                request.Prefix = path;
                request.Delimiter = "/";

                ListObjectsResponse response = client.ListObjectsAsync(request).Result;
                folderList = response.CommonPrefixes;
            }

            if (folderList.Any())
            {
                foreach (var folder in folderList)
                {
                    total += RecurseForFoldersSize(folder);
                }
            }
            else
            {
                total += GetFolderSize(path);
            }

            return total;
        }

        private long GetFolderSize(string path)
        {
            long total = 0;

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = _context.Bucket;
                request.Prefix = path;
                request.Delimiter = "/";
                do
                {
                    ListObjectsResponse response = client.ListObjectsAsync(request).Result;

                    if (response != null && response.S3Objects != null)
                        total += response.S3Objects.Sum(s => s.Size);

                    if (response.IsTruncated)
                    {
                        request.Marker = response.NextMarker;
                    }
                    else
                    {
                        request = null;
                    }
                } while (request != null);
            }

            return total;
        }
        #endregion

    }
}
