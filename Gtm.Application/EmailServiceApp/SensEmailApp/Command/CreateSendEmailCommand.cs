using ErrorOr;
 
using Gtm.Contract.EmailContract.SensEmailContract.Command;
using Gtm.Domain.EmailDomain.SendEmailAgg;
using MediatR;
 
namespace Gtm.Application.EmailServiceApp.SensEmailApp.Command
{
    public record CreateSendEmailCommand(CreateSendEmail Command) : IRequest<ErrorOr<bool>>;
    public class CreateSendEmailCommandHandler : IRequestHandler<CreateSendEmailCommand, ErrorOr<bool>>
    {
        private readonly ISendEmailRepository _sendEmailRepository;
        public CreateSendEmailCommandHandler(ISendEmailRepository sendEmailRepository)
        {
            _sendEmailRepository = sendEmailRepository;
        }
        public async Task<ErrorOr<bool>> Handle(CreateSendEmailCommand request, CancellationToken cancellationToken)
        {
            SendEmail email = new(request.Command.Title, request.Command.Text);
            await _sendEmailRepository.AddAsync(email);
            var res=await _sendEmailRepository.SaveChangesAsync(cancellationToken); 
            if (res) return true;
            return false;
        }
    }
}
