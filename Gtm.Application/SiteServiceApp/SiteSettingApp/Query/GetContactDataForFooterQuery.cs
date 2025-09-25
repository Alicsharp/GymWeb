using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Query;
using MediatR;
 

namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetContactDataForFooterQuery : IRequest<ErrorOr<ContactFooterUiQueryModel>>;

    public class GetContactDataForFooterQueryHandler : IRequestHandler<GetContactDataForFooterQuery, ErrorOr<ContactFooterUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;

        public GetContactDataForFooterQueryHandler(ISiteSettingRepository siteSettingRepository)
        {
            _siteSettingRepository = siteSettingRepository;
        }

        public async Task<ErrorOr<ContactFooterUiQueryModel>> Handle(GetContactDataForFooterQuery request, CancellationToken cancellationToken)
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

                return new ContactFooterUiQueryModel(
                    site.Address,
                    site.Phone1,
                    site.Email1,
                    site.Android,
                    site.IOS);
            }
            catch (OperationCanceledException)
            {
                return Error.Conflict(
                    code: "Operation.Cancelled",
                    description: "عملیات دریافت اطلاعات فوتر لغو شد");
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "SiteSettings.FetchError",
                    description: $"خطا در دریافت اطلاعات تماس فوتر: {ex.Message}");
            }
        }
    }
}
