using ErrorOr;
using Gtm.Contract.EmailContract.EmailUserContract.Command;


namespace Gtm.Application.EmailServiceApp.EmailUserApp
{
    public interface IEmailUserValidator
    {
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateEmailUser command);
        Task<ErrorOr<Success>> ValidateGetForAdminAsync(int pageId, int take, string filter);
    }
    public class EmailUserValidator : IEmailUserValidator
    {
        private readonly IEmailUserRepository _emailUserRepository;

        public EmailUserValidator(IEmailUserRepository emailUserRepository)
        {
            _emailUserRepository = emailUserRepository;
        }

        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه
            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidId",
                    "شناسه کاربر ایمیل نامعتبر است"));
            }

            // اعتبارسنجی وجود کاربر
            if (id > 0 && !await _emailUserRepository.ExistsAsync(e => e.Id == id))
            {
                errors.Add(Error.NotFound(
                    "EmailUser.NotFound",
                    "کاربر ایمیل مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateEmailUser command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی ایمیل
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                errors.Add(Error.Validation(
                    "EmailUser.EmailRequired",
                    "آدرس ایمیل الزامی است"));
            }
            else if (!IsValidEmail(command.Email))
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidEmail",
                    "فرمت ایمیل وارد شده معتبر نیست"));
            }
            else if (await _emailUserRepository.ExistsAsync(e =>
                e.Email.Trim().ToLower() == command.Email.Trim().ToLower()))
            {
                errors.Add(Error.Conflict(
                    "EmailUser.EmailExists",
                    "این ایمیل قبلا ثبت شده است"));
            }

            // اعتبارسنجی UserId
            if (command.UserId <= 0)
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidUserId",
                    "شناسه کاربر نامعتبر است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
         public async Task<ErrorOr<Success>> ValidateGetForAdminAsync(int pageId, int take, string filter)
        {
            var errors = new List<Error>();

            // اعتبارسنجی صفحه
            if (pageId <= 0)
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidPage",
                    "شماره صفحه باید بزرگتر از صفر باشد"));
            }

            // اعتبارسنجی تعداد آیتم‌ها
            if (take <= 0 || take > 100)
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidTake",
                    "تعداد آیتم‌ها باید بین 1 تا 100 باشد"));
            }

            // اعتبارسنجی فیلتر
            if (!string.IsNullOrEmpty(filter) && filter.Length < 3)
            {
                errors.Add(Error.Validation(
                    "EmailUser.InvalidFilter",
                    "حداقل طول فیلتر جستجو باید 3 کاراکتر باشد"));
            }

            return errors.Any() ? errors : Result.Success;
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
    }      
  }
 

