using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserApp.Command
{
    public record ActivationChangeCommand(int id):IRequest<ErrorOr<Success>>;
    public class ActivationChangeCommandHandler : IRequestHandler<ActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IUserRepo _userRepo;
        private readonly IUserValidator _validator;

        public ActivationChangeCommandHandler(IUserRepo userRepo, IUserValidator validator)
        {
            _userRepo = userRepo;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(ActivationChangeCommand request, CancellationToken cancellationToken)
        {
            var validatinResults = await _validator.ValidateIdAsync(request.id);
            if (validatinResults.IsError)
            {
                return validatinResults.Errors;
            }
            var user = await _userRepo.GetByIdAsync(request.id);
            user.ActivationChange();
            var Saved= await _userRepo.SaveChangesAsync(cancellationToken); 
            if(!Saved)
            {
                return Error.Failure("NotSaved", "عملیات ذخیره سازی با شکست مواجه شد");
            }
            return Result.Success;  

        }
    }
}
