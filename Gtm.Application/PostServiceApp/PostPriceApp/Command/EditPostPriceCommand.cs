using ErrorOr;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostPriceApp.Command
{
    public record EditPostPriceCommand(EditPostPrice Command) : IRequest<ErrorOr<Success>>;

    public class EditPostPriceCommandHandler : IRequestHandler<EditPostPriceCommand, ErrorOr<Success>>
    {
        private readonly IPostPriceRepo _postPriceRepository;
        private readonly IPostPriceValidation _validation;

        public EditPostPriceCommandHandler(IPostPriceRepo postPriceRepository,IPostPriceValidation validation)
        {
            _postPriceRepository = postPriceRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(
            EditPostPriceCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // دریافت رکورد موجود
                var existingPostPrice = await _postPriceRepository.GetByIdAsync(request.Command.Id);

                // اعتبارسنجی
                var validationResult = await _validation.ValidateEditPostPrice(request.Command, existingPostPrice);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // اعمال تغییرات
                existingPostPrice.Edit(
                    request.Command.Start,
                    request.Command.End,
                    request.Command.TehranPrice,
                    request.Command.StateCenterPrice,
                    request.Command.CityPrice,
                    request.Command.InsideStatePrice,
                    request.Command.StateClosePrice,
                    request.Command.StateNonClosePrice);

                // ذخیره تغییرات
                var result = await _postPriceRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "PostPrice.UpdateFailed",
                        description: "خطا در بروزرسانی قیمت پست");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "PostPrice.UpdateError",
                    description: $"خطای غیرمنتظره در ویرایش قیمت پست: {ex.Message}");
            }
        }
    }
}
