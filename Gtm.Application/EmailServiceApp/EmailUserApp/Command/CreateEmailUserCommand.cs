using ErrorOr;
 
using Gtm.Contract.EmailContract.EmailUserContract.Command;
using Gtm.Domain.EmailDomain.EmailUserAgg;
using MediatR;
 

namespace Gtm.Application.EmailServiceApp.EmailUserApp.Command
{
    public record CreateEmailUserCommand(CreateEmailUser Command) : IRequest<ErrorOr<bool>>;
    // CreateEmailUserCommandHandler.cs
    namespace Gtm.Application.EmailServiceApp.EmailUserApp.Command
    {
        public class CreateEmailUserCommandHandler : IRequestHandler<CreateEmailUserCommand, ErrorOr<bool>>
        {
            private readonly IEmailUserRepository _emailUserRepository;
            private readonly IEmailUserValidator _validator;

            public CreateEmailUserCommandHandler(IEmailUserRepository emailUserRepository,IEmailUserValidator validator)
            {
                _emailUserRepository = emailUserRepository;
                _validator = validator;
            }

            public async Task<ErrorOr<bool>> Handle(CreateEmailUserCommand request,CancellationToken cancellationToken)
            {
                // اعتبارسنجی
                var validationResult = await _validator.ValidateCreateAsync(request.Command);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                try
                {
                    var emailUser = new EmailUser(
                        request.Command.UserId,
                        request.Command.Email.Trim().ToLower());

                    await _emailUserRepository.AddAsync(emailUser);
                    var createResult = await _emailUserRepository.SaveChangesAsync(cancellationToken);

                    if (!createResult)
                    {
                        return Error.Failure(
                            "EmailUser.CreateFailed",
                            "خطا در ایجاد کاربر ایمیل");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return Error.Failure(
                        "EmailUser.UnexpectedError",
                        $"خطای غیرمنتظره در ایجاد کاربر ایمیل: {ex.Message}");
                }
            }
        }
    }
}
