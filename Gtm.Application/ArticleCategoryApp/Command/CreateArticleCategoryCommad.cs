using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record CreateArticleCategoryCommand(CreateArticleCategory CategoryDto) : IRequest<ErrorOr<bool>>;

    public class CreateArticleCategoryCommandHandler : IRequestHandler<CreateArticleCategoryCommand, ErrorOr<bool>>
    {
        private readonly IArticleCategoryRepo _articleCategoryRepo;
        private readonly IArticleCategoryValidator _validator;
        private readonly IFileService _fileService;

        public CreateArticleCategoryCommandHandler(IArticleCategoryRepo articleCategoryRepo, IArticleCategoryValidator validator, IFileService fileService)
        {
            _articleCategoryRepo = articleCategoryRepo;
            _validator = validator;
            _fileService = fileService;
        }

        public async Task<ErrorOr<bool>> Handle(CreateArticleCategoryCommand request, CancellationToken cancellationToken)
        {
       
            // 1. اعتبارسنجی DTO
            var validationResult = await _validator.ValidateCreateAsync(request.CategoryDto);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
            if (request.CategoryDto.Parent>0)
            {
                var parent = await _articleCategoryRepo.GetByIdAsync(request.CategoryDto.Parent.Value);
                if (parent == null || parent.ParentId != null)
                {
                    return Error.Validation("Parent.Invalid", "شناسه والد معتبر نیست یا خودش فرزند است.");
                }
            }
            string imageName = await _fileService.UploadImageAsync(request.CategoryDto.ImageFile, FileDirectories.ArticleCategoryImageFolder); // عتبار سنجی نشده
            if (imageName == null)
                return Error.Failure("ImapgeUploading", "بارگزاری عکس به شکست خورد");
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 400);
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleCategoryImageFolder, 100);

            // 2. ایجاد موجودیت
            var category = new ArticleCategory(
                title: request.CategoryDto.Title,
                slug: Utility.Appliation.Slug.SlugUtility.GenerateSlug(request.CategoryDto.Title),
                imageName:  imageName,
                imageAlt: request.CategoryDto.ImageAlt,
                parentId: request.CategoryDto.Parent);

            // 3. بررسی تکراری نبودن slug
          
            //if (await _articleCategoryRepo.SlugExistsAsync(category.Slug))
            //{
            //    return Error.Conflict(
            //        code: "Category.SlugExists",
            //        description: "اسلاگ دسته‌بندی تکراری است");
            //}
            //if (await _articleCategoryRepo.ExistsAsync(c => c.Slug == category.Slug))
            //{
            //    return Error.Conflict(
            //        code: "Category.SlugExists",
            //        description: "اسلاگ دسته‌بندی تکراری است");
            //}

            // 4. ذخیره در دیتابیس
            await _articleCategoryRepo.AddAsync(category);
           var Created= await _articleCategoryRepo.SaveChangesAsync(cancellationToken);
            if(!Created)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory400}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleCategoryImageDirectory100}{imageName}");

                return Error.Failure("Category.SaveFailed", "ثبت دسته‌بندی با خطا مواجه شد.");
            }
            return true;
        }

      
    }
}
