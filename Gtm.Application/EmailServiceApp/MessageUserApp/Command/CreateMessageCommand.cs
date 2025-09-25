using ErrorOr;
using Gtm.Contract.EmailContract.MessageUserContract.Command;
using Gtm.Domain.EmailDomain.MessageUserAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.EmailServiceApp.MessageUserApp.Command
{
    public record CreateMessageCommand(CreateMessageUser Command) : IRequest<ErrorOr<bool>>;
    public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, ErrorOr<bool>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        public CreateMessageCommandHandler(IMessageUserRepository messageUserRepository)
        {
            _messageUserRepository = messageUserRepository;
        }
        public async Task<ErrorOr<bool>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
        {
            MessageUser messageUser = new(request.Command.UserId, request.Command.FullName, request.Command.Subject,
             request.Command.PhoneNumber, request.Command.Email, request.Command.Message);
            await _messageUserRepository.AddAsync(messageUser);
            if (await _messageUserRepository.SaveChangesAsync(cancellationToken))
                return true;
            return false;
        }
    }
}
