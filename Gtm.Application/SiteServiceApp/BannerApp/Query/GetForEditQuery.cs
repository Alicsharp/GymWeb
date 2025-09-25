using ErrorOr;
 
using Gtm.Contract.SiteContract.BanarContract.Command;
using MediatR;
 

namespace Gtm.Application.SiteServiceApp.BannerApp.Query
{
    public record GetForEditQuery(int Id) : IRequest<ErrorOr<EditBaner>>;

    public class GetForEditQueryHandler : IRequestHandler<GetForEditQuery, ErrorOr<EditBaner>>
    {
        private readonly IBanerRepository _banerRepository;
        private readonly IBannerValidator _bannerValidator;

        public GetForEditQueryHandler(IBanerRepository banerRepository, IBannerValidator bannerValidator)
        {
            _banerRepository = banerRepository;
            _bannerValidator = bannerValidator;
        }

        public async Task<ErrorOr<EditBaner>> Handle(GetForEditQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _bannerValidator.ValidateGetForEditAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
         
            // دریافت داده با استفاده از متدهای موجود در IRepository
            var banner = await _banerRepository.GetByIdAsync(request.Id);

            // تبدیل به مدل EditBaner
            var result = new EditBaner
            {
                Id = request.Id,
                ImageFile = null,
                ImageName = banner.ImageName,
                ImageAlt = banner.ImageAlt  ,
                Url= banner.Url,
                //State= banner.State,


            };
        
            return result;
        }
    }

}
