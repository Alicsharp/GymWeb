using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.PostServiceApp.UserPostApp.Query
{
    public record GetAllPackageQuery : IRequest<ErrorOr<List<PackageAdminQueryModel>>>;

    public class GetAllPackageQueryHandle : IRequestHandler<GetAllPackageQuery, ErrorOr<List<PackageAdminQueryModel>>>
    {
        private readonly IPackageRepo _packageRepository;

        public GetAllPackageQueryHandle(IPackageRepo packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task<ErrorOr<List<PackageAdminQueryModel>>> Handle(GetAllPackageQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // دریافت کوئری از ریپوزیتوری
                var query =   _packageRepository.GetAllQueryable();

                // بررسی وجود کوئری
                if (query == null)
                    return Error.NotFound("Package.QueryNull", "خطا در دریافت اطلاعات بسته‌ها");

                var packages = await query
                    .Select(p => new PackageAdminQueryModel(
                        p.Id,
                        p.Title,
                        p.Count,
                        p.Price,
                        p.CreateDate.ToPersainDate(),
                        p.UpdateDate != null ? p.UpdateDate.ToPersainDate() : null,
                        p.IsActive,
                        $"{FileDirectories.PackageImageDirectory100}{p.ImageName}" // Corrected this line
                    ))
                    .ToListAsync();

                // بررسی وجود بسته‌ها
                if (packages == null || !packages.Any())
                    return Error.NotFound("Package.ListEmpty", "هیچ بسته‌ای یافت نشد");

                return packages;
            }
            catch (ArgumentNullException ex)
            {
                return Error.Validation("Package.NullArgument", $"خطای مقدار نال: {ex.ParamName}");
            }
            catch (InvalidOperationException ex)
            {
                return Error.Conflict("Package.InvalidOperation", $"خطای عملیاتی: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Error.Failure("Package.GeneralError", $"خطای سرور: {ex.Message}");
            }
        }
    }
}
