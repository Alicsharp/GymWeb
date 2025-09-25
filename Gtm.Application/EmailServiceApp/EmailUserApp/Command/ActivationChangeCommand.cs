using ErrorOr;
using Gtm.Application.EmailServiceApp.EmailUserApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.EmailServiceApp.EmailUserApp.Command
{
    public record ActivationChangeCommand(int Id) : IRequest<ErrorOr<bool>>;
    public class ActivationChangeCommandHandler : IRequestHandler<ActivationChangeCommand, ErrorOr<bool>>
    {
        private readonly IEmailUserRepository _emailUserRepository;
        private readonly IEmailUserValidator _validator;

        public ActivationChangeCommandHandler(IEmailUserRepository emailUserRepository,IEmailUserValidator validator)
        {
            _emailUserRepository = emailUserRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<bool>> Handle(ActivationChangeCommand request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateActivationChangeAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                var emailUser = await _emailUserRepository.GetByIdAsync(request.Id);
                emailUser.ActivationChange();

                var saveResult = await _emailUserRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    return Error.Failure(
                        "EmailUser.SaveFailed",
                        "تغییر وضعیت فعالسازی با خطا مواجه شد");
                }

                return true;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "EmailUser.ActivationChangeError",
                    $"خطا در تغییر وضعیت فعالسازی: {ex.Message}");
            }
        }
    }
}
