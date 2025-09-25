using ErrorOr;
using Gtm.Application.EmailServiceApp.MessageUserApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.EmailServiceApp.MessageUserApp.Command
{
    public record AnsweredByEmailCommand(int Id, string MailMessage) : IRequest<ErrorOr<bool>>;
    public class AnsweredByEmailCommandHandler : IRequestHandler<AnsweredByEmailCommand, ErrorOr<bool>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        public AnsweredByEmailCommandHandler(IMessageUserRepository messageUserRepository)
        {
            _messageUserRepository = messageUserRepository;
        }
        public async Task<ErrorOr<bool>> Handle(AnsweredByEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var messageUser = await _messageUserRepository.GetByIdAsync(request.Id);
                messageUser.AnswerEmailSend(request.MailMessage);
                await _messageUserRepository.SaveChangesAsync(cancellationToken);
                //
                // send sms
                //
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
