using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Application.SiteServiceApp.SiteSettingApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.SeoContract.Query;
using Gtm.Contract.UiContract;
using MediatR;
using Utility.Domain.Enums;


namespace Gtm.Application.SiteServiceApp.SiteServiceApp.Query
{
    public record GetAboutUsModelForUiQuery : IRequest<ErrorOr<AboutUsUiQueryModel>>;
    public class GetAboutUsModelForUiQueryHandler : IRequestHandler<GetAboutUsModelForUiQuery, ErrorOr<AboutUsUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly ISeoRepository _seoRepository;

        public GetAboutUsModelForUiQueryHandler(ISiteSettingRepository siteSettingRepository, ISeoRepository seoRepository)
        {
            _siteSettingRepository = siteSettingRepository;
            _seoRepository = seoRepository;
        }


        public async Task<ErrorOr<AboutUsUiQueryModel>> Handle(GetAboutUsModelForUiQuery request, CancellationToken cancellationToken)
        {
            var site = await _siteSettingRepository.GetSingleAsync();
            List<BreadCrumbQueryModel> breadcrums = new List<BreadCrumbQueryModel>()
            {
              new BreadCrumbQueryModel(){Number = 1,Title = "صفحه اصلی",Url = "/"},
              new BreadCrumbQueryModel() {Number = 2,Title = "درباره ما",Url =""}
            };
            var seo = await _seoRepository.GetSeoForUiAsync(0, WhereSeo.About, "درباره ما");
            SeoUiQueryModel seoModel = new(seo.MetaTitle, seo.MetaDescription, seo.MetaKeyWords, seo.IndexPage, seo.Canonical, seo.Schema);
            return new AboutUsUiQueryModel(site.AboutTitle, site.AboutDescription, seoModel, breadcrums);
        }
    }
}
