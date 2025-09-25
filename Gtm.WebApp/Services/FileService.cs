using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Utility.Appliation.FileService;


namespace Gtm.WebApp.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _environment = webHostEnvironment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null) return "";

            var directory = $"{_environment.WebRootPath}//Images//{folder}";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var fileName = $"{Guid.NewGuid()}{file.FileName}";

            var path = $"{directory}//{fileName}";
            using (var output = File.Create(path))
            {
                try
                {
                    file.CopyTo(output);
                    return fileName;
                }
                catch
                {
                    return "";
                }
            }
        }

        // حذف تصویر به صورت غیرهمزمان
        public async Task<bool> DeleteImageAsync(string route)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, route);

                // استفاده از Task.Run برای انجام عملیات حذف به صورت غیرهمزمان
                return await Task.Run(() =>
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);  // عملیات حذف فایل
                    }
                    return true;
                });
            }
            catch (Exception)
            {
                return false;  // در صورت بروز خطا، false برگشت داده می‌شود
            }
        }

        // تغییر اندازه تصویر به صورت غیرهمزمان
        public async Task<bool> ResizeImageAsync(string imageName, string Folder, int newSize)
        {
            var directory = $"{_environment.WebRootPath}/Images/{Folder}/{imageName}";
            var newDirectory = $"{_environment.WebRootPath}/Images/{Folder}/{newSize}";

            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            try
            {
                using var image = Image.Load(directory);
                image.Mutate(x => x.Resize(newSize, 0));

                image.Save(newDirectory + "/" + imageName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
