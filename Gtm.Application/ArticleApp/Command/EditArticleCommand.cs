using ErrorOr;
using Gtm.Contract.ArticleContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Appliation.Slug;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.ArticleApp.Command
{
    public record EditArticleCommand(UpdateArticleDto command):IRequest<ErrorOr<Success>>;
    public class EditArticleCommandHandler : IRequestHandler<EditArticleCommand, ErrorOr<Success>>
    {
        private readonly IArticleRepo _articleRepo;
        private readonly IFileService _fileService;
        private readonly IArticleValidator _articleValidator;

        public EditArticleCommandHandler(IArticleRepo articleRepo, IFileService fileService, IArticleValidator articleValidator)
        {
            _articleRepo = articleRepo;
            _fileService = fileService;
            _articleValidator = articleValidator;
        }

        public async Task<ErrorOr<Success>> Handle(EditArticleCommand request, CancellationToken cancellationToken)
        {
            // 1. اول ولیدیشن (صرفه‌جویی در منابع دیتابیس)
            var validationResult = await _articleValidator.ValidateUpdateAsync(request.command);
            if (validationResult.IsError)
                return validationResult.Errors;

            // 2. دریافت مقاله
            var article = await _articleRepo.GetByIdAsync(request.command.Id);
            if (article is null)
                return Error.NotFound("Article.NotFound", "مقاله مورد نظر یافت نشد.");

            // نگهداری مقادیر عکس برای مدیریت فایل‌ها
            string currentImageName = article.ImageName; // عکس فعلی در دیتابیس
            string newImageName = null; // عکس جدیدی که شاید آپلود شود
            bool imageChanged = false;

            // تولید اسلاگ
            request.command.Slug = request.command.Slug.GenerateSlug();

            // 3. لاجیک آپلود عکس جدید
            if (request.command.ImageFile is not null)
            {
                newImageName = await _fileService.UploadImageAsync(request.command.ImageFile, FileDirectories.ArticleImageFolder);

                if (string.IsNullOrWhiteSpace(newImageName))
                    return Error.Failure("Article.Image.UploadFailed", "آپلود تصویر با مشکل مواجه شد.");

                // ریسایز (بهتر است متدهای فایل سرویس هم کنسل‌یشن توکن بگیرند)
                await _fileService.ResizeImageAsync(newImageName, FileDirectories.ArticleImageFolder, 400);
                await _fileService.ResizeImageAsync(newImageName, FileDirectories.ArticleImageFolder, 100);

                imageChanged = true;
            }

            // 4. ویرایش انتیتی (اگر عکس جدید بود، آن را می‌فرستیم، وگرنه همان قدیمی)
            article.Edit(
                request.command.Title,
                request.command.Slug,
                request.command.ShortDescription,
                request.command.Text,
                imageChanged ? newImageName : currentImageName, // انتخاب عکس درست
                request.command.ImageAlt,
                request.command.CategoryId,
                request.command.SubCategoryId,
                request.command.Writer
            );

            // 5. تلاش برای ذخیره
            // نکته: CancellationToken را پاس بده
            if (await _articleRepo.SaveChangesAsync(cancellationToken))
            {
                // ✅ موفقیت: اگر عکس عوض شده، عکس قدیمی را پاک کن
                if (imageChanged)
                {
                    await DeleteImages(currentImageName);
                }
                return Result.Success;
            }

            // ❌ شکست: اگر ذخیره نشد ولی عکس جدید آپلود کرده بودیم، عکس جدید را پاک کن (Rollback)
            if (imageChanged && newImageName != null)
            {
                await DeleteImages(newImageName);
            }

            return Error.Failure("Blog.SaveFailed", "خطا در ذخیره‌سازی تغییرات مقاله.");
        }

        // یک متد کمکی برای جلوگیری از تکرار کد حذف
        private async Task DeleteImages(string imageName)
        {
            // فرض بر این است که این ثابت‌ها اسلش پایانی دارند، اگر ندارند از Path.Combine استفاده کن
            await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageFolder}{imageName}");
            await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory400}{imageName}");
            await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory100}{imageName}");
        }
    }

}
