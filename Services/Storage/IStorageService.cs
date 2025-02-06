namespace ASP_P22.Services.Storage
{
    public interface IStorageService
    {
        string Save(IFormFile formFile);
        Stream? Get(string fileUrl);
        bool Delete(string fileUrl);
    }
}
