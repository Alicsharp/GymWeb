using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.EmailServiceApp.MessageUserApp.Command
{
    public record AnsweredBySMSCommand(int Id, string Message) : IRequest<ErrorOr<bool>>;
    public class AnsweredBySMSCommandHandler : IRequestHandler<AnsweredBySMSCommand, ErrorOr<bool>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        public AnsweredBySMSCommandHandler(IMessageUserRepository messageUserRepository)
        {
            _messageUserRepository = messageUserRepository;
        }
        public async Task<ErrorOr<bool>> Handle(AnsweredBySMSCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var messageUser = await _messageUserRepository.GetByIdAsync(request.Id);
                messageUser.AnswerSmsSend(request.Message);
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
