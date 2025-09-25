using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.EmailContract.EmailUserContract.Query;
using Gtm.Domain.EmailDomain.EmailUserAgg;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using Utility.Appliation;


namespace Gtm.Application.EmailServiceApp.EmailUserApp.Query
{
    public record GetEmailUsersForAdminQuery(int pageId, int take, string filter) : IRequest<ErrorOr<EmailUserAdminPaging>>;
    public class GetEmailUsersForAdminQueryHandler : IRequestHandler<GetEmailUsersForAdminQuery, ErrorOr<EmailUserAdminPaging>>
    {
        private readonly IEmailUserRepository  _emailUserRepository;
        private readonly IUserRepo _userRepository;
        private readonly IEmailUserValidator _validator;

        public GetEmailUsersForAdminQueryHandler(IEmailUserRepository  emailUserRepository,IUserRepo userRepository,IEmailUserValidator validator)
        {
            _emailUserRepository = emailUserRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EmailUserAdminPaging>> Handle(
            GetEmailUsersForAdminQuery request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetForAdminAsync(
                request.pageId,
                request.take,
                request.filter);

            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // دریافت تمام ایمیل‌ها از ریپازیتوری
                var allEmails = await _emailUserRepository.GetAllAsync();

                // مرتب‌سازی بر اساس شناسه
                var orderedEmails = allEmails.OrderByDescending(e => e.Id).AsQueryable();

                // اعمال فیلتر اگر وجود دارد
                if (!string.IsNullOrEmpty(request.filter))
                {
                    orderedEmails = orderedEmails
                        .Where(r => r.Email.ToLower().Contains(request.filter.ToLower()))
                        .OrderByDescending(r => r.Id);
                }

                // ایجاد مدل صفحه‌بندی
                EmailUserAdminPaging model = new();
                model.GetData(orderedEmails, request.pageId, request.take, 5);
                model.Filter = request.filter;
                model.Emails = new List<EmailUserAdminQueryModel>();

                // اگر داده وجود دارد
                if (orderedEmails.Any())
                {
                    // تبدیل به مدل نمایشی
                    model.Emails = orderedEmails
                        .Skip(model.Skip)
                        .Take(model.Take)
                        .Select(e => new EmailUserAdminQueryModel
                        {
                            CreationDate = e.CreateDate.ToPersainDate(),
                            Email = e.Email,
                            Id = e.Id,
                            UserId = e.UserId,
                            UserName = "",
                            Active = e.Active
                        })
                        .OrderByDescending(e => e.Id)
                        .ToList();

                    // پر کردن نام کاربران
                    foreach (var email in model.Emails)
                    {
                        if (email.UserId > 0)
                        {
                            var user = await _userRepository.GetByIdAsync(email.UserId);
                            email.UserName = string.IsNullOrEmpty(user?.FullName) ? user?.Mobile : user?.FullName;
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "EmailUser.FetchError",
                    description: $"خطا در دریافت لیست ایمیل‌ها: {ex.Message}");
            }
        }
    }
}
