using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetFooterQuery : IRequest<ErrorOr<FooterUiQueryModel>>;

    public sealed class GetFooterQueryHandler : IRequestHandler<GetFooterQuery, ErrorOr<FooterUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;

        public GetFooterQueryHandler(ISiteSettingRepository siteSettingRepository)
        {
            _siteSettingRepository = siteSettingRepository ?? throw new ArgumentNullException(nameof(siteSettingRepository));
        }

        public async Task<ErrorOr<FooterUiQueryModel>> Handle(GetFooterQuery request, CancellationToken cancellationToken)
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

                return new FooterUiQueryModel(
                    site.Enamad,
                    site.SamanDehi,
                    site.FooterTitle,
                    site.FooterDescription);
            }

            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "SiteSettings.FetchError",
                    description: $"خطا در دریافت اطلاعات فوتر: {ex.Message}");
            }
        }
    }
}
