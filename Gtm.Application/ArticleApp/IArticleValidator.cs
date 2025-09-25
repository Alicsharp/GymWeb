using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleCategoryContract.Command;
using Gtm.Contract.ArticleContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ArticleApp
{
    public interface IArticleValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateArticleDto dto);
        Task<ErrorOr<Success>> ValidateUpdateAsync(UpdateArticleDto dto);
        Task<ErrorOr<Success>> ValidateIdAsync(int id);
        Task<ErrorOr<Success>> ValidateGetBlogsForUiAsync(string slug, int pageId, string filter);
    }
    public class ArticleValidator : IArticleValidator
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleCategoryRepo _categoryRepository;

        public ArticleValidator(IArticleRepo articleRepo, IArticleCategoryRepo categoryRepository)
        {
            _articleRepo = articleRepo;
            _categoryRepository = categoryRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateArticleDto dto)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add(Error.Validation("Article.TitleRequired", "عنوان مقاله الزامی است."));

            if (dto.Title?.Length > 150)
                errors.Add(Error.Validation("Article.TitleTooLong", "عنوان مقاله نباید بیشتر از 150 کاراکتر باشد."));

            if (!await _categoryRepository.ExistsAsync(c => c.Id == dto.CategoryId))
                errors.Add(Error.Validation("Article.CategoryNotFound", "دسته‌بندی اصلی وجود ندارد."));

            //if (dto.SubCategoryId.HasValue)
            //{
            //    if (!await _categoryRepository.ExistsAsync(c => c.Id == dto.SubCategoryId.Value))
            //        errors.Add(Error.Validation("Article.SubCategoryNotFound", "زیر دسته‌بندی وجود ندارد."));

            //    // بررسی: آیا SubCategory واقعا فرزند CategoryId است؟
            //    if (!await _categoryRepository.ExistsAsync(c =>
            //        c.Id == dto.SubCategoryId.Value && c.ParentId == dto.CategoryId))
            //    {
            //        errors.Add(Error.Validation("Article.SubCategoryRelationInvalid", "زیر دسته‌بندی به این دسته‌بندی اصلی تعلق ندارد."));
            //    }
            //}

            //if (dto.TagIds.Any() && !await _articleRepo.AllTagsExistAsync(dto.TagIds))
            //    errors.Add(Error.Validation("Article.TagsInvalid", "یکی از تگ‌ها نامعتبر است."));

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateUpdateAsync(UpdateArticleDto dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add(Error.Validation("Article.TitleRequired", "عنوان مقاله الزامی است."));

            if (dto.Title?.Length > 150)
                errors.Add(Error.Validation("Article.TitleTooLong", "عنوان مقاله نباید بیشتر از 150 کاراکتر باشد."));

            // اعتبارسنجی توضیحات کوتاه
            if (string.IsNullOrWhiteSpace(dto.ShortDescription))
                errors.Add(Error.Validation("Article.ShortDescriptionRequired", "توضیحات کوتاه الزامی است."));

            if (dto.ShortDescription?.Length > 500)
                errors.Add(Error.Validation("Article.ShortDescriptionTooLong", "توضیحات کوتاه نباید بیشتر از 500 کاراکتر باشد."));

            // اعتبارسنجی محتوا
            if (string.IsNullOrWhiteSpace(dto.Text))
                errors.Add(Error.Validation("Article.ContentRequired", "محتویات مقاله الزامی است."));

            // اعتبارسنجی دسته‌بندی
            if (!await _categoryRepository.ExistsAsync(c => c.Id == dto.CategoryId))
                errors.Add(Error.Validation("Article.CategoryNotFound", "دسته‌بندی اصلی وجود ندارد."));

            // اعتبارسنجی زیردسته‌بندی (در صورت وجود)
            if (dto.SubCategoryId>0)
            {
                if (!await _categoryRepository.ExistsAsync(c => c.Id == dto.SubCategoryId))
                    errors.Add(Error.Validation("Article.SubCategoryNotFound", "زیر دسته‌بندی وجود ندارد."));

                // بررسی رابطه زیردسته با دسته اصلی
                if (!await _categoryRepository.ExistsAsync(c =>
                    c.Id == dto.SubCategoryId && c.ParentId == dto.CategoryId))
                {
                    errors.Add(Error.Validation("Article.SubCategoryRelationInvalid",
                        "زیر دسته‌بندی به این دسته‌بندی اصلی تعلق ندارد."));
                }
            }

          

            // اعتبارسنجی تصویر (اختیاری در آپدیت)
            if (dto.ImageFile != null)
            {
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(dto.ImageFile.FileName).ToLower();

                if (!validExtensions.Contains(fileExtension))
                {
                    errors.Add(Error.Validation("Article.InvalidImageFormat",
                        "فرمت تصویر نامعتبر است. فقط فرمت‌های JPG, JPEG, PNG, GIF قابل قبول هستند."));
                }

                if (dto.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Article.ImageTooLarge",
                        "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // اعتبارسنجی ImageAlt در صورت وجود تصویر
            if (dto.ImageFile != null && string.IsNullOrWhiteSpace(dto.ImageAlt))
            {
                errors.Add(Error.Validation("Article.ImageAltRequired",
                    "متن جایگزین تصویر الزامی است وقتی تصویر آپلود می‌شود."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateIdAsync(int id)
        {
            var errors = new List<Error>();
            if (id < 0 && id == null)
            {
                return Error.Validation("Category.Id.Invalid", "شناسه دسته‌بندی معتبر نیست.");
            }
            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetBlogsForUiAsync(string slug, int pageId, string filter)
        {
            var errors = new List<Error>();

            // اعتبارسنجی صفحه
            if (pageId <= 0)
            {
                errors.Add(Error.Validation("Article.InvalidPage", "شماره صفحه نامعتبر است"));
            }

            // اعتبارسنجی اسلاگ (اگر وجود دارد)
            if (!string.IsNullOrEmpty(slug))
            {
                var categoryExists = await _categoryRepository.GetBySlugAsync(slug);
                if (categoryExists == null)
                {
                    errors.Add(Error.NotFound(
                        "Article.CategoryNotFound",
                        "دسته بندی مورد نظر یافت نشد"));
                }
            }

            // اعتبارسنجی فیلتر (اگر وجود دارد)
            if (!string.IsNullOrEmpty(filter) && filter.Length < 3)
            {
                errors.Add(Error.Validation(
                    "Article.InvalidFilter",
                    "حداقل طول فیلتر جستجو باید 3 کاراکتر باشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
