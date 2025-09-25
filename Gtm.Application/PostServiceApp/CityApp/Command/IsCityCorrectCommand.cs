using ErrorOr;
using MediatR;

namespace Gtm.Application.PostServiceApp.CityApp.Command
{
    public record IsCityCorrectCommand(int stateId, int cityId) : IRequest<ErrorOr<Success>>;
    public class IsCityCorrectCommandHandler : IRequestHandler<IsCityCorrectCommand, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _Validation;

        public IsCityCorrectCommandHandler(ICityRepo cityRepo, ICityValidation validation)
        {
            _cityRepo = cityRepo;
            _Validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(IsCityCorrectCommand request, CancellationToken cancellationToken)
        {
            var validationresult = await _Validation.IsCityCorrectValidation(request.stateId,request.cityId);
            if (validationresult.IsError) return validationresult.Errors;

          var result= await _cityRepo.ExistsAsync(c => c.Id == request.cityId && c.StateId == request.stateId);
            if (result == false)
                return Error.Failure("CityNotCorrect", " این شهر وجود ندارد یا در این استان نیست");

            return Result.Success;

        }
    }
}
