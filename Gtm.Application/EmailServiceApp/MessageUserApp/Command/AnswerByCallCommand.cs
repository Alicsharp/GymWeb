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
    public record AnswerByCallCommand(int Id) : IRequest<ErrorOr<bool>>;
    public class AnswerByCallCommandHandler : IRequestHandler<AnswerByCallCommand, ErrorOr<bool>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        public AnswerByCallCommandHandler(IMessageUserRepository messageUserRepository)
        {
            _messageUserRepository = messageUserRepository;
        }
        public async Task<ErrorOr<bool>> Handle(AnswerByCallCommand request, CancellationToken cancellationToken)
        {
            var messageUser = await _messageUserRepository.GetByIdAsync(request.Id);
            messageUser.AnswerByCall();
            return await _messageUserRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
