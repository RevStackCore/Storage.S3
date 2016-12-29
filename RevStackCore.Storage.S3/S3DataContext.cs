using System;
using Amazon;
using Amazon.Runtime;

namespace RevStackCore.Storage.S3
{
    public class S3DataContext
    {
        public S3DataContext(string bucket, string cdn, string accessKey, string secretKey, RegionEndpoint region)
        {
            this.Bucket = bucket;
            this.CDN = cdn;
            this.AccessKey = accessKey;
            this.SecretKey = secretKey;
            this.Region = region;
            this.Credentials = new BasicAWSCredentials(accessKey, secretKey);
        }

        public string Bucket { get; set; }
        public string CDN { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public RegionEndpoint Region { get; set; }
        public AWSCredentials Credentials { get; set; }

    }
}
