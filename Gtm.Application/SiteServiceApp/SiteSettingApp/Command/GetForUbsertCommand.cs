using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using MediatR;
 
namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Command
{
    public record GetForUbsertCommand : IRequest<ErrorOr<UbsertSiteSetting>>;

    public class GetForUbsertCommandHandler : IRequestHandler<GetForUbsertCommand, ErrorOr<UbsertSiteSetting>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly ISiteSettingValidator _validator;

        public GetForUbsertCommandHandler(ISiteSettingRepository siteSettingRepository,ISiteSettingValidator validator)
        {
            _siteSettingRepository = siteSettingRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<UbsertSiteSetting>> Handle(GetForUbsertCommand request,CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForUpsertAsync();
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                var result = await _siteSettingRepository.GetForUbsertAsync();
                if(result ==null)
                return  Error.NotFound(
                    "SiteSettings.NotFound",
                    "تنظیمات سایت یافت نشد");
                return result;
            }
            catch (OperationCanceledException)
            {
                return Error.Conflict(
                    "Operation.Cancelled",
                    "عملیات دریافت تنظیمات لغو شد");
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "SiteSettings.FetchError",
                    $"خطا در دریافت تنظیمات سایت: {ex.Message}");
            }
        }
    }
}
