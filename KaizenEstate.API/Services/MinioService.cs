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
            // Настройки подключения к MinIO
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

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // 3. Загружаем поток
            using var stream = file.OpenReadStream();
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            return $"http://localhost:9000/{BucketName}/{fileName}";
        }

        // === МЕТОД УДАЛЕНИЯ ===
        public async Task DeleteFileAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var uri = new Uri(imageUrl);
                // uri.LocalPath вернет "/apartments/d34d3-fsd3.jpg"
                var fileName = Path.GetFileName(uri.LocalPath); // вернет "d34d3-fsd3.jpg"

                var args = new RemoveObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления файла из MinIO: {ex.Message}");
            }
        }
    }
}