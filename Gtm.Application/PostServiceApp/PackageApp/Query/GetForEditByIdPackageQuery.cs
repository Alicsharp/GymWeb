using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PackageApp.Query
{
    public record GetForEditByIdPackageQuery(int id) : IRequest<ErrorOr<EditPackage>>;

    public class GetForEditByIdPackageQueryHandler : IRequestHandler<GetForEditByIdPackageQuery, ErrorOr<EditPackage>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IPackageValidation _validator;

        public GetForEditByIdPackageQueryHandler(IPackageRepo packageRepository,IPackageValidation validator)
        {
            _packageRepository = packageRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EditPackage>> Handle(GetForEditByIdPackageQuery request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetForEditPackage(request.id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // دریافت بسته از دیتابیس
                var package = await _packageRepository.GetByIdAsync(request.id);

                // مپ کردن به مدل ویرایش
                return new EditPackage
                {
                    Id = package.Id,
                    Title = package.Title,
                    Count = package.Count,
                    Description = package.Description ?? string.Empty,
                    Price = package.Price
                };
            }
            catch (Exception ex)
            {
                return Error.Failure("Package.GeneralError",
                    $"خطای سرور هنگام دریافت بسته: {ex.Message}");
            }
        }
    }
}
