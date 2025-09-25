using ErrorOr;
using Gtm.Contract.PostContract.PostContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Command
{
    public record EditPostCommand(EditPost Command) : IRequest<ErrorOr<Success>>;

    public class EditPostCommandHandler : IRequestHandler<EditPostCommand, ErrorOr<Success>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public EditPostCommandHandler(IPostRepo postRepository,IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<Success>> Handle(EditPostCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateEditPost(request.Command);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت پست موجود
                var post = await _postRepository.GetByIdAsync(request.Command.Id);

                // اعمال تغییرات
                post.Edit(
                    request.Command.Title,
                    request.Command.Status,
                    request.Command.TehranPricePlus,
                    request.Command.StateCenterPricePlus,
                    request.Command.CityPricePlus,
                    request.Command.InsideStatePricePlus,
                    request.Command.StateClosePricePlus,
                    request.Command.StateNonClosePricePlus,
                    request.Command.Description);

                // ذخیره تغییرات
                var result = await _postRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "Post.UpdateFailed",
                        description: "خطا در بروزرسانی پست");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Post.UnexpectedError",
                    description: $"خطای غیرمنتظره در ویرایش پست: {ex.Message}");
            }
        }
    }
}
