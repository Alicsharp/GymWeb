using ErrorOr;
using Gtm.Contract.SiteContract.SiteServiceContract.Query;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Query
{
    public record GetAllSiteSrviceForAdminQuery : IRequest<ErrorOr<List<SiteServiceAdminQueryModel>>>;

    public class GetAllSiteSrviceForAdminQueryHandler : IRequestHandler<GetAllSiteSrviceForAdminQuery, ErrorOr<List<SiteServiceAdminQueryModel>>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;

        public GetAllSiteSrviceForAdminQueryHandler(ISiteServiceRepository siteServiceRepository)
        {
            _siteServiceRepository = siteServiceRepository;
        }

        public async Task<ErrorOr<List<SiteServiceAdminQueryModel>>> Handle(GetAllSiteSrviceForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. دریافت لیست تمام سرویس‌ها از مخزن
                var services =   _siteServiceRepository.GetAllQueryable();

                // 2. تبدیل سرویس‌ها به مدل نمایش ادمین
                var result = services.Select(s => new SiteServiceAdminQueryModel(
                    s.Id,
                    s.Title,
                    FileDirectories.ServiceImageDirectory100 + s.ImageName,
                    s.ImageAlt,
                    s.CreateDate.ToPersianDate(), // فرض بر این است که متد ToPersianDate() وجود دارد.
                    s.Active
                )).ToList();

                // 3. بازگرداندن لیست مدل‌های نمایش ادمین
                return result;
            }
            catch (Exception ex)
            {
                // 4. بروز خطای غیرمنتظره
                return Error.Unexpected(
                    code: "SiteService.GetAllAdminFailed",
                    description: $"در هنگام دریافت لیست سرویس‌ها برای ادمین خطایی رخ داد: {ex.Message}"
                );
            }
        }
    }
}
