using ErrorOr;
using Gtm.Contract.SiteContract.SiteServiceContract.Query;
using MediatR;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Query
{
    public record GetAllForUIQuery : IRequest<ErrorOr<List<SiteServiceUIQueryModel>>>;

    public class GetAllForUIQueryHandler : IRequestHandler<GetAllForUIQuery, ErrorOr<List<SiteServiceUIQueryModel>>>
    {
        private readonly ISiteServiceRepository _siteServiceRepository;

        public GetAllForUIQueryHandler(ISiteServiceRepository siteServiceRepository)
        {
            _siteServiceRepository = siteServiceRepository;
        }
        public async Task<ErrorOr<List<SiteServiceUIQueryModel>>> Handle(GetAllForUIQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. دریافت لیست سرویس‌های فعال از مخزن
                var services = await _siteServiceRepository.GetAllByQueryAsync(s => s.Active);

                // 2. تبدیل سرویس‌ها به مدل نمایش UI
                var result = services.Select(s => new SiteServiceUIQueryModel(
                    s.Id,
                    s.Title,
                    FileDirectories.ServiceImageDirectory + s.ImageName,
                    s.ImageAlt
                )).ToList();

                // 3. بازگرداندن لیست مدل‌های نمایش UI
                return result;
            }
            catch (Exception ex)
            {
                // 4. بروز خطای غیرمنتظره
                return Error.Unexpected(
                    code: "SiteService.GetAllFailed",
                    description: $"در هنگام دریافت لیست سرویس‌ها خطایی رخ داد: {ex.Message}"
                );
            }
        }
    }
}
