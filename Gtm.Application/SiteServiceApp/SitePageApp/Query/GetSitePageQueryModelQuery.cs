using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.SeoContract.Query;
using Gtm.Contract.UiContract;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.SitePageApp.Query
{
    public record GetSitePageQueryModelQuery(string slug) : IRequest<ErrorOr<SitePageUiQueryModel>>;

    public class GetSitePageQueryModelQueryHandler : IRequestHandler<GetSitePageQueryModelQuery, ErrorOr<SitePageUiQueryModel>>
    {
        private readonly ISeoRepository _seoRepository;
        private readonly ISitePageRepository _sitePageRepository;
        private readonly ISitePageValidator _validator;

        public GetSitePageQueryModelQueryHandler(ISeoRepository seoRepository,ISitePageRepository sitePageRepository,ISitePageValidator validator)
        {
            _seoRepository = seoRepository;
            _sitePageRepository = sitePageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<SitePageUiQueryModel>> Handle(GetSitePageQueryModelQuery request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetBySlugAsync(request.slug);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                var site = await _sitePageRepository.GetBySlugAsync(request.slug);
                var seo = await _seoRepository.GetSeoForUiAsync(site.Id, WhereSeo.Page, site.Title);

                SeoUiQueryModel seoModel = new(
                    seo.MetaTitle,
                    seo.MetaDescription,
                    seo.MetaKeyWords,
                    seo.IndexPage,
                    seo.Canonical,
                    seo.Schema);

                List<BreadCrumbQueryModel> breadCrumbs = new()
            {
                new BreadCrumbQueryModel() { Number = 1, Title = "صفحه اصلی", Url = "/" },
                new BreadCrumbQueryModel() { Number = 2, Title = site.Title, Url = "" }
            };

                return new SitePageUiQueryModel(
                    site.Id,
                    site.Title,
                    site.Slug,
                    site.Description,
                    seoModel,
                    breadCrumbs);
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "SitePage.FetchError",
                    description: $"خطا در دریافت اطلاعات صفحه: {ex.Message}");
            }
        }
    }
}
