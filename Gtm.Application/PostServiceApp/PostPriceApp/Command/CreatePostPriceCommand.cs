using ErrorOr;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using Gtm.Domain.PostDomain.PostPriceAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostPriceApp.Command
{

    public record CreatePostPriceCommand(CreatePostPrice command) : IRequest<ErrorOr<Success>>;
    public class CreatePostPriceCommandHandler : IRequestHandler<CreatePostPriceCommand, ErrorOr<Success>>
    {
        private readonly IPostPriceRepo _postPriceRepository;
        private readonly IPostPriceValidation _validation;

        public CreatePostPriceCommandHandler(IPostPriceRepo postPriceRepository,IPostPriceValidation validation)
        {
            _postPriceRepository = postPriceRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(CreatePostPriceCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = _validation.ValidateCreatePostPrice(request.command);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // ایجاد قیمت جدید
                var postPrice = new PostPrice(
                    request.command.PostId,
                    request.command.Start,
                    request.command.End,
                    request.command.TehranPrice,
                    request.command.StateCenterPrice,
                    request.command.CityPrice,
                    request.command.InsideStatePrice,
                    request.command.StateClosePrice,
                    request.command.StateNonClosePrice);

                // ذخیره در ریپازیتوری
                 await _postPriceRepository.AddAsync(postPrice);
                var result = await _postPriceRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "PostPrice.CreateFailed",
                        description: "خطا در ثبت قیمت پست");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "PostPrice.UnexpectedError",
                    description: $"خطای غیرمنتظره در ثبت قیمت پست: {ex.Message}");
            }
        }
    }
}
