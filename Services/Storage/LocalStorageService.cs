using ASP_P22.Services.Random;

namespace ASP_P22.Services.Storage
{
    public class LocalStorageService(IRandomService randomService) : IStorageService
    {
        private readonly IRandomService _randomService = randomService;

        const string STORAGE_PATH = "Storage/";
        public bool Delete(string fileUrl)
        {
            return true;
        }

        public Stream? Get(string fileUrl)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), STORAGE_PATH, fileUrl);
            if (File.Exists(fullPath))
            {
                return new FileStream(fullPath, FileMode.Open);
            }
            return null;
        }

        public string Save(IFormFile formFile)
        {
            ArgumentNullException.ThrowIfNull(formFile);
            if(formFile.Length == 0)
            {
                throw new InvalidDataException("Empty file");
            }
            string extension = Path.GetExtension(formFile.FileName);

            string savedName = _randomService.FileName() + extension;
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), STORAGE_PATH, savedName);
            
            using (FileStream fileStream = new FileStream(fullPath, FileMode.CreateNew)) 
            {
                formFile.CopyTo(fileStream);
            }
            return savedName;
        }
    }
}
