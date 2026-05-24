using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GramVetCRM.Service
{
    public class R2StorageService : IR2StorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _config;
        private readonly ILogger<R2StorageService> _logger;
        private readonly string _bucketName;
        private readonly string _publicUrl;

        public R2StorageService(
            IConfiguration config,
            ILogger<R2StorageService> logger)
        {
            _config = config;
            _logger = logger;
            _bucketName = config["CloudflareR2:BucketName"]!;
            _publicUrl = config["CloudflareR2:PublicUrl"]!;

            var accountId = config["CloudflareR2:AccountId"]!;
            var accessKeyId = config["CloudflareR2:AccessKeyId"]!;
            var secretAccessKey = config["CloudflareR2:SecretAccessKey"]!;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };

            _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, s3Config);
        }

        public async Task<string?> SubirArchivo(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = fileStream,
                    ContentType = contentType,
                    DisablePayloadSigning = true
                };

                await _s3Client.PutObjectAsync(request);

                var publicFileUrl = $"{_publicUrl}/{fileName}";
                _logger.LogInformation("Archivo subido a R2: {Url}", publicFileUrl);
                return publicFileUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subiendo archivo a R2: {FileName}", fileName);
                return null;
            }
        }

        public async Task EliminarArchivo(string fileName)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation("Archivo eliminado de R2: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando archivo de R2: {FileName}", fileName);
            }
        }
    }
}