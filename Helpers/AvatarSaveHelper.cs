using System;
using System.IO;
using System.Text;
using Aliyun.OSS.Common;
using Aliyun.OSS;

namespace MeshBackend.Helpers
{
    public static class AvatarSaveHelper
    {
        private const string endpoint = "";
        private const string accessKeyId = "";
        private const string accessKeysecret = "";
        private const string bucketName = "";
        private const int maxLength =int.MaxValue;

        public static string PutObject(string objectContent,string name=null)
        {
            var objectName = name ?? Guid.NewGuid().ToString();
            var client = new OssClient(endpoint, accessKeyId, accessKeysecret);
            try
            {
                var binaryData = Encoding.ASCII.GetBytes(objectContent);
                var requestContent = new MemoryStream(binaryData);
                client.PutObject(bucketName, objectName, requestContent);
                return objectName;
            }
            catch
            {
                return name ?? objectName;
            }
        }

        public static string GetObject(string objectName)
        {
            var client = new OssClient(endpoint, accessKeyId, accessKeysecret);
            try
            {
                var obj = client.GetObject(bucketName, objectName);
                using (var requestStream = obj.Content)
                {
                    var buf = new byte[maxLength];
                    var len = requestStream.Read(buf, 0, maxLength);
                    var str = Encoding.ASCII.GetString(buf);
                    return str;
                }
            }
            catch 
            {
                return null;
            }
        }
        
    }
}