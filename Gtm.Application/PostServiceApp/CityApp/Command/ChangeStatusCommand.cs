using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.PostServiceApp.CityApp.Command
{
    public record ChangeStatusCommand(int id, CityStatus status) : IRequest<ErrorOr<Success>>;
    public class ChangeStatusCommandHnadler : IRequestHandler<ChangeStatusCommand, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _cityValidatio;

        public ChangeStatusCommandHnadler(ICityRepo cityRepository, ICityValidation cityValidatio)
        {
            _cityRepo = cityRepository;
            _cityValidatio = cityValidatio;
        }

        public async Task<ErrorOr<Success>> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
        {
            var validationResult= await _cityValidatio.ChangeStatusValidation(request.id, request.status);  
            if(validationResult.IsError) return validationResult.Errors;
             
            var res = await _cityRepo.ChangeStatusAsync(request.id, request.status);
            if(res==true)
            {
                var result= await _cityRepo.SaveChangesAsync(cancellationToken);
                if(result==true) {
                    return Result.Success;
                }
            }
            return Error.Failure("ChangeStatus","تغییروضعیت شهر شکست ممکن نبود");
        }
    }
}
