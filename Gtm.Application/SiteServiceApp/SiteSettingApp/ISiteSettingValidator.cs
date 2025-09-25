
using Gtm.Contract.SiteContract.SiteSettingContract.Command;
using ErrorOr;
using Utility.Appliation.FileService;
using Utility.Appliation;


namespace Gtm.Application.SiteServiceApp.SiteSettingApp
{
    public interface ISiteSettingValidator
    {
        Task<ErrorOr<Success>> ValidateUpsertAsync(UbsertSiteSetting command); 
        Task<ErrorOr<Success>> ValidateGetForUpsertAsync();
    }
    public class SiteSettingValidator : ISiteSettingValidator
    {
        private readonly IFileService _fileService;
        private readonly ISiteSettingRepository _siteSettingRepository;

        public SiteSettingValidator(IFileService fileService, ISiteSettingRepository siteSettingRepository)
        {
            _fileService = fileService;
            _siteSettingRepository = siteSettingRepository;
        }

        public async Task<ErrorOr<Success>> ValidateUpsertAsync(UbsertSiteSetting command)
        {
            var errors = new List<Error>();

            // Logo validation
            if (command.LogoFile != null)
            {
                if (!command.LogoFile.IsImage())
                {
                    errors.Add(Error.Validation("SiteSetting.InvalidLogo", "فایل لوگو باید تصویر باشد"));
                }
                else if (command.LogoFile.Length > 2 * 1024 * 1024) // 2MB
                {
                    errors.Add(Error.Validation("SiteSetting.LogoTooLarge", "حجم لوگو نباید بیشتر از 2 مگابایت باشد"));
                }

                if (string.IsNullOrWhiteSpace(command.LogoAlt))
                {
                    errors.Add(Error.Validation("SiteSetting.LogoAltRequired", "متن جایگزین لوگو الزامی است"));
                }
            }

            // FavIcon validation
            if (command.FavIconFile != null)
            {
                if (!command.FavIconFile.IsImage())
                {
                    errors.Add(Error.Validation("SiteSetting.InvalidFavIcon", "فایل فاوآیکون باید تصویر باشد"));
                }
                else if (command.FavIconFile.Length > 1 * 1024 * 1024) // 1MB
                {
                    errors.Add(Error.Validation("SiteSetting.FavIconTooLarge", "حجم فاوآیکون نباید بیشتر از 1 مگابایت باشد"));
                }
            }

            // Social media validation
            if (!string.IsNullOrWhiteSpace(command.Instagram) && !Uri.TryCreate(command.Instagram, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidInstagram", "لینک اینستاگرام معتبر نیست"));
            }

            if (!string.IsNullOrWhiteSpace(command.WhatsApp) && !Uri.TryCreate(command.WhatsApp, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidWhatsApp", "لینک واتساپ معتبر نیست"));
            }

            // Phone validation
            if (!string.IsNullOrWhiteSpace(command.Phone1) && !IsValidPhoneNumber(command.Phone1))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidPhone1", "شماره تلفن 1 معتبر نیست"));
            }

            if (!string.IsNullOrWhiteSpace(command.Phone2) && !IsValidPhoneNumber(command.Phone2))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidPhone2", "شماره تلفن 2 معتبر نیست"));
            }

            // Email validation
            if (!string.IsNullOrWhiteSpace(command.Email1) && !IsValidEmail(command.Email1))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidEmail1", "ایمیل 1 معتبر نیست"));
            }

            if (!string.IsNullOrWhiteSpace(command.Email2) && !IsValidEmail(command.Email2))
            {
                errors.Add(Error.Validation("SiteSetting.InvalidEmail2", "ایمیل 2 معتبر نیست"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        private bool IsValidPhoneNumber(string phone)
        {
            // Implement phone number validation logic
            return !string.IsNullOrWhiteSpace(phone) && phone.Length >= 10;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
      

        public async Task<ErrorOr<Success>> ValidateGetForUpsertAsync()
        {
            // No specific validation needed for this query
            // Could add checks for required settings if needed
            return Result.Success;
        }
    }
}
