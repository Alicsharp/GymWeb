using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Utility.Appliation
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(ErrorMessage ?? $"حجم فایل نباید بیشتر از {_maxFileSize / (1024 * 1024)} مگابایت باشد.");
                }
            }
            else if (value is IEnumerable<IFormFile> files)
            {
                foreach (var f in files)
                {
                    if (f.Length > _maxFileSize)
                    {
                        return new ValidationResult(ErrorMessage ?? $"حجم فایل نباید بیشتر از {_maxFileSize / (1024 * 1024)} مگابایت باشد.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
