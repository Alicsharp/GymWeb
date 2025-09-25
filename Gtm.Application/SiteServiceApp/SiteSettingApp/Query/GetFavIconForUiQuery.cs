using ErrorOr;
 
using Gtm.Contract.SiteContract.SiteSettingContract.Query;
using MediatR;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetFavIconForUiQuery : IRequest<ErrorOr<FavIconForUiQueryModel>>;
    public class GetFavIconForUiQueryHandler : IRequestHandler<GetFavIconForUiQuery, ErrorOr<FavIconForUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;

        public GetFavIconForUiQueryHandler(ISiteSettingRepository siteSettingRepository)
        {
            _siteSettingRepository = siteSettingRepository;
        }

        public async Task<ErrorOr<FavIconForUiQueryModel>> Handle(GetFavIconForUiQuery request, CancellationToken cancellationToken)
        {
            var site = await _siteSettingRepository.GetSingleAsync();
            if (site == null) return Error.NotFound(code: "Site.GetFavICon");
            return new FavIconForUiQueryModel(string.IsNullOrEmpty(site.FavIcon) ? "" : FileDirectories.SiteImageDirectory64 + site.FavIcon);
        }
    }
}
