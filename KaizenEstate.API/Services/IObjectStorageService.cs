namespace KaizenEstate.API.Services
{
    public interface IObjectStorageService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}