using System;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using RevStackCore.Storage.Model;
using RevStackCore.Storage.Repository;

namespace RevStackCore.Storage.S3
{
    public class S3FileRepository : IFileRepository
    {
        private readonly S3DataContext _context;

        public S3FileRepository(S3DataContext context)
        {
            _context = context;
        }

        public IFile Add(byte[] byteArray, string path)
        {
            path = Util.GetFilePath(path, false);

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                using (var stream = new MemoryStream(byteArray))
                {
                    PutObjectRequest request = new PutObjectRequest();
                    request.Key = path;
                    request.InputStream = stream;
                    request.BucketName = _context.Bucket;
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.StorageClass = S3StorageClass.Standard;

                    PutObjectResponse response = client.PutObjectAsync(request).Result;
                }
            }

            return Get(path);
        }

        public void Delete(string path)
        {
            path = Util.GetFilePath(path, false);

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                DeleteObjectRequest request = new DeleteObjectRequest();
                request.BucketName = _context.Bucket;
                request.Key = path;
                //if you are deleting any specific version of document then specify version if otherwise remove below line
                //request.VersionId = "YourObjectVersionId";
                client.DeleteObjectAsync(request);
            }
        }

        public IFile Get(string path)
        {
            path = Util.GetFilePath(path, false);

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                GetObjectRequest getObjectRequest = new GetObjectRequest();
                getObjectRequest.BucketName = _context.Bucket;
                getObjectRequest.Key = path;
                //specify the version id if you want to download specific version otherwise remove this file
                //getObjectRequest.VersionId = VersionId;

                GetObjectResponse getObjectResponse = client.GetObjectAsync(getObjectRequest).Result;

                File file = new File();
                file.Name = getObjectResponse.Key;
                file.Size = getObjectResponse.ContentLength;
                file.Url = _context.CDN + "/" + path;

                return file;
            }
        }

        public IFile Update(byte[] byteArray, string path, string destination)
        {
            path = Util.GetFilePath(path, false);
            destination = Util.GetFilePath(destination, false);

            using (AmazonS3Client client = new AmazonS3Client(_context.Credentials, _context.Region))
            {
                using (var stream = new MemoryStream(byteArray))
                {
                    PutObjectRequest request = new PutObjectRequest();
                    request.Key = path;
                    request.InputStream = stream;
                    request.BucketName = _context.Bucket;
                    request.CannedACL = S3CannedACL.PublicRead;
                    request.StorageClass = S3StorageClass.Standard;

                    client.PutObjectAsync(request);

                    if (path != destination)
                    {
                        CopyObjectRequest copyRequest = new CopyObjectRequest();
                        copyRequest.SourceBucket = _context.Bucket;
                        copyRequest.SourceKey = path;
                        copyRequest.DestinationBucket = _context.Bucket;
                        copyRequest.DestinationKey = destination;

                        client.CopyObjectAsync(copyRequest);

                        //delete old file
                        Delete(path);
                        //return new key
                        return Get(destination);
                    }
                }
            }

            return Get(path);
        }

    }
}
