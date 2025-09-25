using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Query;
using MediatR;
 

namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetSocialForUiQuery : IRequest<ErrorOr<SocialForUiQueryModel>>;

    public sealed class GetSocialForUiQueryHandler : IRequestHandler<GetSocialForUiQuery, ErrorOr<SocialForUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;

        public GetSocialForUiQueryHandler(ISiteSettingRepository siteSettingRepository)
        {
            _siteSettingRepository = siteSettingRepository ?? throw new ArgumentNullException(nameof(siteSettingRepository));
        }

        public async Task<ErrorOr<SocialForUiQueryModel>> Handle(GetSocialForUiQuery request,CancellationToken cancellationToken)
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

                return new SocialForUiQueryModel(
                    site.Instagram,
                    site.WhatsApp,
                    site.Telegram,
                    site.Youtube);
            }

            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Social.FetchError",
                    description: $"خطا در دریافت اطلاعات شبکه‌های اجتماعی: {ex.Message}");
            }
        }
    }
}
