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
            var article = await _articleRepo.GetByIdAsync(request.command.Id);
            if (article is null)
                return Error.NotFound("Article.NotFound", "مقاله مورد نظر یافت نشد.");

            var validationResult = await _articleValidator.ValidateUpdateAsync(request.command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
            string imageName = article.ImageName;
            string oldImageName = article.ImageName;
            bool imageChanged = false;
            request.command.Slug =request.command.Slug.GenerateSlug();
            if (request.command.ImageFile is not null)
            {
                imageName = await _fileService.UploadImageAsync(request.command.ImageFile, FileDirectories.ArticleImageFolder);
                if (string.IsNullOrWhiteSpace(imageName))
                    return Error.Failure("Article.Image.UploadFailed", "آپلود تصویر با مشکل مواجه شد.");

                await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleImageFolder, 400);
                await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleImageFolder, 100);
                imageChanged = true;
            }

            article.Edit(request.command.Title, request.command.Slug, request.command.ShortDescription, request.command.Text, imageName, request.command.ImageAlt,
                request.command.CategoryId, request.command.SubCategoryId, request.command.Writer);
            if (await _articleRepo.SaveChangesAsync())
            {
               
                if (imageChanged)
                {
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageFolder}{oldImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory400}{oldImageName}");
                    await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory100}{oldImageName}");
                }
                return Result.Success;

            }

            // Clean up old image if new one was uploaded
            if (imageChanged)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageFolder}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory400}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory100}{imageName}");
            }
                return Error.Failure("Blog.SaveFailed", "خطا در ذخیره‌سازی تغییرات مقاله.");

        }
    }
}
