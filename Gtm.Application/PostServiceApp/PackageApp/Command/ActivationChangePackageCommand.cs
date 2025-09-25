using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PackageApp.Command
{
    public record ActivationChangePackageCommand(int Id) : IRequest<ErrorOr<Success>>;

    public partial class ActivationChangePackageCommandHandler : IRequestHandler<ActivationChangePackageCommand, ErrorOr<Success>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IPackageValidation _packageValidation;

        public ActivationChangePackageCommandHandler(IPackageRepo packageRepository,IPackageValidation packageValidation)
        {
            _packageRepository = packageRepository;
            _packageValidation = packageValidation;
        }

        public async Task<ErrorOr<Success>> Handle(ActivationChangePackageCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _packageValidation.ValidatePackageForActivation(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت پکیج
                var package = await _packageRepository.GetByIdAsync(request.Id);

                // تغییر وضعیت فعال‌سازی
                package.ActivationChange();

                // ذخیره تغییرات
                var saveResult = await _packageRepository.SaveChangesAsync(cancellationToken);

                return saveResult
                    ? Result.Success
                    : Error.Failure(
                        code: "Package.SaveFailed",
                        description: "خطا در ذخیره تغییرات پکیج");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Package.ActivationError",
                    description: $"خطا در تغییر وضعیت پکیج: {ex.Message}");
            }
        }
    }
}
