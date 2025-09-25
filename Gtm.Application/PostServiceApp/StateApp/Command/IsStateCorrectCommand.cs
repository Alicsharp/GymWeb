using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Command
{
    public record IsStateCorrectCommand(int StateId) : IRequest<ErrorOr<Success>>;

    public class IsStateCorrectCommandHandler : IRequestHandler<IsStateCorrectCommand, ErrorOr<Success>>
    {
        private readonly IStateValidation _validation;

        public IsStateCorrectCommandHandler(IStateValidation validation)
        {
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(IsStateCorrectCommand request,CancellationToken cancellationToken)
        {
            return await _validation.ValidateStateExists(request.StateId);
        }
        //return   await _stateRepository.ExistByAsync(c => c.Id == request.stateId);
    }
}
