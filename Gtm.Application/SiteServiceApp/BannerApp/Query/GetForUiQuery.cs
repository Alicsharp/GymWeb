using ErrorOr;
using Gtm.Contract.SiteContract.BanarContract.Query;
using Gtm.Domain.SiteDomain.BannerAgg;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation.FileService;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.BannerApp.Query
{
    public record GetForUiQuery(int Count, BanerState State) : IRequest<ErrorOr<List<BanerForUiQueryModel>>>;

    public class GetForUiQueryHandler : IRequestHandler<GetForUiQuery, ErrorOr<List<BanerForUiQueryModel>>>
    {
        private readonly IBanerRepository _banerRepository;
        private readonly IBannerValidator _bannerValidator;

        public GetForUiQueryHandler(IBanerRepository banerRepository, IBannerValidator bannerValidator)
        {
            _banerRepository = banerRepository;
            _bannerValidator = bannerValidator;
        }

        public async Task<ErrorOr<List<BanerForUiQueryModel>>> Handle(GetForUiQuery request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = _bannerValidator.ValidateGetForUi(request.Count, request.State);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // استفاده از متد QueryBy موجود در ریپازیتوری
                var bannersQuery = _banerRepository.QueryBy(b =>
                    b.State == request.State &&
                    b.Active)
                    .Take(request.Count)
                    .Select(b => new BanerForUiQueryModel
                    {
                        ImageAlt = b.ImageAlt,
                        ImageName = $"{FileDirectories.BanerImageDirectory}{b.ImageName}",
                        Url = b.Url
                    });

                var result = await bannersQuery.ToListAsync(cancellationToken);

                return result.Any()
                    ? result
                    : Error.NotFound("Banner.NotFound", "هیچ بنر فعالی با این وضعیت یافت نشد.");
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Banner.FetchFailed",
                    description: $"خطا در دریافت بنرها: {ex.Message}");
            }
        }
    }
}
