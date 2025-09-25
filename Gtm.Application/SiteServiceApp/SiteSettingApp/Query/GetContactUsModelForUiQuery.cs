using ErrorOr;
 
using Gtm.Application.SeoApp;
using Gtm.Application.SiteServiceApp.SiteSettingApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.SeoContract.Query;
using Gtm.Contract.UiContract;
using MediatR;
using System;
 
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.SiteSettingApp.Query
{
    public record GetContactUsModelForUiQuery : IRequest<ErrorOr<ContactUsUiQueryModel>>;
    public class GetContactUsModelForUiQueryHandler : IRequestHandler<GetContactUsModelForUiQuery, ErrorOr<ContactUsUiQueryModel>>
    {
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly ISeoRepository _seoRepository;
        public GetContactUsModelForUiQueryHandler(ISiteSettingRepository siteSettingRepository, ISeoRepository seoRepository)
        {
            _siteSettingRepository = siteSettingRepository;
            _seoRepository = seoRepository;
        }

        public async Task<ErrorOr<ContactUsUiQueryModel>> Handle(GetContactUsModelForUiQuery request, CancellationToken cancellationToken)
        {
            var site = await _siteSettingRepository.GetSingleAsync();

            List<BreadCrumbQueryModel> breadcrums = new()
{
              new BreadCrumbQueryModel(){Number = 1,Title = "صفحه اصلی",Url = "/"},
             new BreadCrumbQueryModel() {Number = 2,Title = "تماس با ما",Url =""}
         };
            var seo = await _seoRepository.GetSeoForUiAsync(site.Id, WhereSeo.Contact, "تماس با ما");
            SeoUiQueryModel seoModel = new(seo.MetaTitle, seo.MetaDescription, seo.MetaKeyWords, seo.IndexPage, seo.Canonical, seo.Schema);
            return new ContactUsUiQueryModel(site.ContactDescription, site.Phone1, site.Phone2, site.Email1, site.Email2, site.Address, seoModel, breadcrums);
        }
    }
}
