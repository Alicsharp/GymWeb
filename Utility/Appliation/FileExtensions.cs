using Microsoft.AspNetCore.Http;

namespace Utility.Appliation
{
    public static class FileExtensions
    {
        public static bool IsImage(this IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // بررسی فرمت فایل بر اساس هدر (نه پسوند!)
            using var stream = file.OpenReadStream();
            byte[] header = new byte[8];
            stream.Read(header, 0, 8);

            // فرمت‌های رایج تصاویر: JPEG, PNG, GIF, BMP
            if (header[0] == 0xFF && header[1] == 0xD8) // JPEG
                return true;
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) // PNG
                return true;
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46) // GIF
                return true;
            if (header[0] == 0x42 && header[1] == 0x4D) // BMP
                return true;

            return false;
        }
    }
}
