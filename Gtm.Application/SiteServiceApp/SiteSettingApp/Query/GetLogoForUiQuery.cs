using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Query;
using MediatR;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetLogoForUiQuery : IRequest<ErrorOr<LogoForUiQueryModel>>;

    public sealed class GetLogoForUiQueryHandler : IRequestHandler<GetLogoForUiQuery, ErrorOr<LogoForUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;

        public GetLogoForUiQueryHandler(ISiteSettingRepository siteSettingRepository)
        {
            _siteSettingRepository = siteSettingRepository ?? throw new ArgumentNullException(nameof(siteSettingRepository));
        }

        public async Task<ErrorOr<LogoForUiQueryModel>> Handle(GetLogoForUiQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var site = await _siteSettingRepository.GetSingleAsync();

                if (site == null)
                {
                    return Error.NotFound(
                        code: "SiteSettings.NotFound",
                        description: "تنظیمات سایت یافت نشد");
                }

                var logoPath = string.IsNullOrEmpty(site.LogoName)
                    ? string.Empty
                    : Path.Combine(FileDirectories.SiteImageDirectory300, site.LogoName);

                return new LogoForUiQueryModel(logoPath, site.LogoAlt);
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Logo.FetchError",
                    description: $"خطا در دریافت لوگو: {ex.Message}");
            }
        }
    }
}
