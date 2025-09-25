using ErrorOr;
using Gtm.Contract.SeoContract.Command;
using MediatR;

using Utility.Domain.Enums;

namespace Gtm.Application.SeoApp.Query
{
    public record GetSeoForEditCommand(int ownerId, WhereSeo where) : IRequest<ErrorOr<CreateSeo>>;

    public class GetSeoForEditCommandHandler : IRequestHandler<GetSeoForEditCommand, ErrorOr<CreateSeo>>
    {
        private readonly ISeoRepository _seoRepository;
        private readonly ISeoValidator _seoValidator;

        public GetSeoForEditCommandHandler(ISeoRepository seoRepository, ISeoValidator seoValidator)
        {
            _seoRepository = seoRepository;
            _seoValidator = seoValidator;
        }

        public async Task<ErrorOr<CreateSeo>> Handle(GetSeoForEditCommand request, CancellationToken cancellationToken)
        {
            var validationResult =await _seoValidator.ValidateGetSeoForEdit(request);
            if (validationResult.IsError) return validationResult.Errors;

            // دریافت داده از ریپوزیتوری به صورت غیر همزمان
            var seo = await _seoRepository.GetSeoForUpsertAsync(request.ownerId, request.where);
            return seo;

        }
    }
}
