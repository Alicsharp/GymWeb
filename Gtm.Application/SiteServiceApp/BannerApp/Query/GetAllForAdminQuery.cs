using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Gtm.Contract.SiteContract.BanarContract.Query;
using Utility.Appliation.FileService;
using Utility.Appliation;

namespace Gtm.Application.SiteServiceApp.BannerApp.Query
{
    public record GetAllForAdminQuery : IRequest<ErrorOr<List<BanerForAdminQueryModel>>>;

    public class GetAllForAdminQueryHandler : IRequestHandler<GetAllForAdminQuery, ErrorOr<List<BanerForAdminQueryModel>>>
    {
        private readonly IBanerRepository _banerRepository;

        public GetAllForAdminQueryHandler(IBanerRepository banerRepository)
        {
            _banerRepository = banerRepository;
        }

        public async Task<ErrorOr<List<BanerForAdminQueryModel>>> Handle(GetAllForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // استفاده از متد GetAllQueryable موجود در ریپازیتوری
                var banners = await _banerRepository.GetAllQueryable()
                    .Select(b => new BanerForAdminQueryModel
                    {
                        Active = b.Active,
                        CreationDate = b.CreateDate.ToPersianDate(),
                        Id = b.Id,
                        ImageName = $"{FileDirectories.BanerImageDirectory100}{b.ImageName}",
                        State = b.State,
                        ImageAlt = b.ImageAlt 
                        
                    }).ToListAsync(cancellationToken);

           
 
                return banners;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Banner.QueryFailed",
                    description: $"خطا در دریافت لیست بنرها: {ex.Message}");
            }
        }
    }
}
 
