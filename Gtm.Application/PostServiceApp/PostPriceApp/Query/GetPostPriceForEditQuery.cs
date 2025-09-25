using ErrorOr;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using MediatR;

namespace Gtm.Application.PostServiceApp.PostPriceApp.Query
{
    public record GetPostPriceForEditQuery(int Id) : IRequest<ErrorOr<EditPostPrice>>;
    public class GetPostPriceForEditQueryHandler : IRequestHandler<GetPostPriceForEditQuery, ErrorOr<EditPostPrice>>
    {
        private readonly IPostPriceRepo _postPriceRepository;
        private readonly IPostPriceValidation _validation;

        public GetPostPriceForEditQueryHandler(IPostPriceRepo postPriceRepository,IPostPriceValidation validation)
        {
            _postPriceRepository = postPriceRepository;
            _validation = validation;
        }
        public async Task<ErrorOr<EditPostPrice>> Handle(GetPostPriceForEditQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _validation.ValidateGetForEdit(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت اطلاعات قیمت پست
                var price = await _postPriceRepository.GetForEditAsync(request.Id);

                // بررسی null با if معمولی
                if (price == null)
                {
                    return Error.NotFound(
                        code: "PostPrice.NotFound",
                        description: "قیمت پست مورد نظر یافت نشد");
                }

                return price;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "PostPrice.FetchError",
                    description: $"خطا در دریافت اطلاعات قیمت پست: {ex.Message}");
            }
        }
    }
}
