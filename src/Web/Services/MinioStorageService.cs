using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace AppAjuntament.Services;

public class MinioStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public MinioStorageService(IConfiguration config)
    {
        var minioConfig = config.GetSection("Minio");
        _bucketName = minioConfig["Bucket"] ?? "patrimoni";
        var endpoint = minioConfig["Endpoint"] ?? "http://localhost:9000";
        var accessKey = minioConfig["AccessKey"] ?? "admin";
        var secretKey = minioConfig["SecretKey"] ?? "adminpassword123";
        var useSSL = bool.TryParse(minioConfig["UseSSL"], out var ssl) && ssl;

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            UseHttp = !useSSL,
        };
        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
    }

    // Expose bucket name for other components to store correct URLs
    public string BucketName => _bucketName;

    public async Task UploadFileAsync(string key, Stream fileStream, string contentType)
    {
        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = fileStream,
            Key = key,
            BucketName = _bucketName,
            ContentType = contentType
        };
        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest);
    }

    public async Task<Stream?> GetFileAsync(string key)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, key);
        return response.ResponseStream;
    }

    public async Task DeleteFileAsync(string key)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, key);
    }
}
