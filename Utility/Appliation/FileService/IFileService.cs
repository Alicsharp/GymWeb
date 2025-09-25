using ErrorOr;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Appliation.FileService
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<bool> DeleteImageAsync(string route);
        Task<bool> ResizeImageAsync(string imageName, string Folder, int newSize); //از مسیر یک به مسیر دو قراره بریزی
    }
}
