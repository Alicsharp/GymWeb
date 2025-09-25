using ErrorOr;
using Gtm.Contract.ArticleContract.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using MediatR;
using Utility.Appliation.FileService;
using Utility.Appliation.Slug;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.ArticleApp.Command
{
    public record CreateArticleCommand(CreateArticleDto command):IRequest<ErrorOr<Success>>;
    public class CreateArticleCommandHandler : IRequestHandler<CreateArticleCommand, ErrorOr<Success>>
    {
       private readonly IArticleRepo _articleRepo;
       private readonly IArticleValidator _articleValidator;
       private readonly IFileService _fileService;

        public CreateArticleCommandHandler(IArticleRepo articleRepo, IArticleValidator articleValidator, IFileService fileService)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
            _fileService = fileService;
        }

        public async Task<ErrorOr<Success>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _articleValidator.ValidateCreateAsync(request.command);
            if(validationResult.IsError)
            {
                return validationResult.Errors;
            }
           request.command.Slug = request.command.Slug.GenerateSlug();
            var imageName=  await _fileService.UploadImageAsync(request.command.ImageFile,FileDirectories.ArticleImageFolder); // عتبار سنجی نشده
            if (imageName == null)
                return Error.Failure("ImapgeUploading", "بارگزاری عکس به شکست خورد");
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleImageFolder, 400);
            await _fileService.ResizeImageAsync(imageName, FileDirectories.ArticleImageFolder, 100);
            Article article =    new(request.command.Title, request.command.Slug, request.command.ShortDescription, request.command.Text, imageName, request.command.ImageAlt,
                   request.command.CategoryId, request.command.SubCategoryId, request.command.UserId, request.command.Writer);

            await _articleRepo.AddAsync(article );


            var saveResult = await _articleRepo.SaveChangesAsync(cancellationToken);

            if (!saveResult)
            {
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory400}{imageName}");
                await _fileService.DeleteImageAsync($"{FileDirectories.ArticleImageDirectory100}{imageName}");
                return Error.Failure(
                    code: "Article.SaveFailed",
                    description: "خطا در ذخیره مقاله");
            }

            return Result.Success;

        }
    }
}
