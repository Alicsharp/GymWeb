using ErrorOr;
using Gtm.Contract.PostContract.CityContract.Command;
using Gtm.Domain.PostDomain.CityAgg;
using MediatR;

namespace Gtm.Application.PostServiceApp.CityApp.Command
{
    public record CreateCityCommand(CreateCityModel command) :IRequest<ErrorOr<Success>>;
    public class CreateCityCommandHandler : IRequestHandler<CreateCityCommand, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _Validation;

        public CreateCityCommandHandler(ICityRepo cityRepo, ICityValidation validation)
        {
            _cityRepo = cityRepo;
            _Validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(CreateCityCommand request, CancellationToken cancellationToken)
        {
            var validationresult = await _Validation.CreateCityValidation(request.command);
            if (validationresult.IsError) return validationresult.Errors;

             City city = new City(request.command.StateId, request.command.Title, request.command.Status);
             await _cityRepo.AddAsync(city);
             var result= await _cityRepo.SaveChangesAsync(cancellationToken);

            if(result==false)
            {
                return Error.Failure("CityNotCreated", "خطایی در ایجاد شهر رخ داده است");
            }

            return Result.Success;
             
        }
    }
}
