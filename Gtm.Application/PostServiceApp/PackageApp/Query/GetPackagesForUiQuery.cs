using ErrorOr;
using Gtm.Application.PostServiceApp.PostSettingApp;
using Gtm.Application.SeoApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.PostContract.PackageContract.Query;
using Gtm.Contract.SeoContract;
using Gtm.Contract.SeoContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.PostServiceApp.PackageApp.Query
{
    public record GetPackagesForUiQuery : IRequest<ErrorOr<PackageUiPageQueryModel>>;
    public class GetPackagesForUiQueryHandler : IRequestHandler<GetPackagesForUiQuery, ErrorOr<PackageUiPageQueryModel>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IPostSettingRepo _postSettingRepository;
        private readonly ISeoRepository _seoRepository;

        public GetPackagesForUiQueryHandler(IPackageRepo packageRepository, IPostSettingRepo postSettingRepository, ISeoRepository seoRepository)
        {
            _packageRepository = packageRepository;
            _postSettingRepository = postSettingRepository;
            _seoRepository = seoRepository;
        }


        public async Task<ErrorOr<PackageUiPageQueryModel>> Handle(GetPackagesForUiQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // اجرای موازی برای عملیات مستقل
                var settingTask = await _postSettingRepository.GetSingleAsync();
                var seoTask = await _seoRepository.GetSeoForUiAsync(0, WhereSeo.PostPackage, "محاسبه هزینه مرسوله های پستی");
                var packagesTask = await GetActivePackages(cancellationToken);



                var setting = settingTask;
                var seo = seoTask;
                var packages = packagesTask;

                var seoModel = new SeoUiQueryModel(
                    seo.MetaTitle,
                    seo.MetaDescription,
                    seo.MetaKeyWords,
                    seo.IndexPage,
                    seo.Canonical,
                    seo.Schema);

                var breadCrumbs = new List<BreadCrumbQueryModel>
            {
                new() { Number = 1, Title = "صفحه اصلی", Url = "/" },
                new() { Number = 2, Title = "محاسبه هزینه مرسوله های پستی", Url = "" }
            };

                return new PackageUiPageQueryModel(
                    packages,
                    breadCrumbs,
                    setting?.PackageTitle,
                    setting?.PackageDescription,
                    seoModel);
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "PackageQuery.Failure",
                    description: $"خطا در دریافت لیست پکیج‌ها: {ex.Message}");
            }
        }

        private async Task<List<PackageUiQueryModel>> GetActivePackages(CancellationToken cancellationToken)
        {
            return await _packageRepository
                .QueryBy(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new PackageUiQueryModel(
                    p.Id,
                    p.Count,
                    p.Price,
                    p.Title,
                    p.Description,
                    p.ImageAlt,
                    p.ImageName != null
                        ? $"{FileDirectories.PackageImageDirectory400}{p.ImageName}"
                        : null))
                .ToListAsync(cancellationToken);
        }
    }

}