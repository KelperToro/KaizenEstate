using Minio;
using Minio.DataModel.Args;

namespace KaizenEstate.API.Services
{
    public class MinioService : IObjectStorageService
    {
        private readonly IMinioClient _minioClient;
        private const string BucketName = "apartments"; 

        public MinioService()
        {
            // Настройки берем из docker-compose (minioadmin / minioadmin)
            _minioClient = new MinioClient()
                .WithEndpoint("localhost", 9000)
                .WithCredentials("minioadmin", "minioadmin")
                .WithSSL(false)
                .Build();
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
                string policy = $@"{{
                  ""Version"": ""2012-10-17"",
                  ""Statement"": [{{
                      ""Action"": [""s3:GetObject""],
                      ""Effect"": ""Allow"",
                      ""Principal"": {{""AWS"": [""*""]}},
                      ""Resource"": [""arn:aws:s3:::{BucketName}/*""]
                  }}]
                }}";
                await _minioClient.SetPolicyAsync(new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy));
            }

            // 2. Генерируем уникальное имя файла (чтобы не затереть старые)
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // 3. Загружаем
            using var stream = file.OpenReadStream();
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs);

            // 4. Возвращаем прямую ссылку на файл
            // Важно: для эмулятора Android localhost - это 10.0.2.2, но браузер компа видит localhost.
            // Пока вернем localhost, так как мы тестируем на Windows.
            return $"http://localhost:9000/{BucketName}/{fileName}";
        }
    }
}