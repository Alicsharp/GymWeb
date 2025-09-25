using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.PostContract.Command;
using Gtm.Domain.PostDomain.Postgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Command
{
    public record CreatePostCommand(CreatePost Command) : IRequest<ErrorOr<Success>>;
    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, ErrorOr<Success>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public CreatePostCommandHandler(IPostRepo postRepository,IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<Success>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateCreatePost(request.Command);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // ایجاد پست جدید
                var post = new Post(
                    request.Command.Title,
                    request.Command.Status,
                    request.Command.TehranPricePlus,
                    request.Command.StateCenterPricePlus,
                    request.Command.CityPricePlus,
                    request.Command.InsideStatePricePlus,
                    request.Command.StateClosePricePlus,
                    request.Command.StateNonClosePricePlus,
                    request.Command.Description);

                // ذخیره پست
              await _postRepository.AddAsync(post);
                var result = await _postRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "Post.CreateFailed",
                        description: "خطا در ایجاد پست جدید");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Post.UnexpectedError",
                    description: $"خطای غیرمنتظره در ایجاد پست: {ex.Message}");
            }
        }
       
    }
}
