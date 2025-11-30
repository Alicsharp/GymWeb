using ErrorOr;

using Gtm.Contract.EmailContract.SensEmailContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;

namespace Gtm.Application.EmailServiceApp.SensEmailApp.Query
{

    public record GetEmailSendsForAdminQuery : IRequest<ErrorOr<List<SendEmailQueryModel>>>;

    public class GetEmailSendsForAdminQueryHandler: IRequestHandler<GetEmailSendsForAdminQuery, ErrorOr<List<SendEmailQueryModel>>>
    {
        private readonly ISendEmailRepository _sendEmailRepository;

        public GetEmailSendsForAdminQueryHandler(ISendEmailRepository sendEmailRepository)
        {
            _sendEmailRepository = sendEmailRepository;
        }

        public async Task<ErrorOr<List<SendEmailQueryModel>>> Handle(GetEmailSendsForAdminQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // استفاده از متد GetAllQueryable برای دریافت IQueryable
                var baseQuery = _sendEmailRepository.GetAllQueryable();

                // ایجاد کوئری با مرتب‌سازی و پروجکشن
                var query = baseQuery
                    .OrderByDescending(x => x.Id)
                    .Select(x => new SendEmailQueryModel
                    {
                        CreationDate = x.CreateDate.ToPersianDate(),
                        Id = x.Id,
                        Title = x.Title
                    });

                // تبدیل به لیست با استفاده از ToListAsync
                var result = await query.ToListAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "EmailQuery.Failed",
                    description: $"خطا در دریافت لیست ایمیل‌ها: {ex.Message}");
            }
        }
    }
}
