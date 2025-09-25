using ErrorOr;
using Gtm.Contract.PostContract.CityContract.Command;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.PostServiceApp.CityApp.Command
{
    public record EditCityCommand(EditCityModel command) : IRequest<ErrorOr<Success>>;
    public class EditCityCommandHandler : IRequestHandler<EditCityCommand, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _Validation;

        public EditCityCommandHandler(ICityRepo cityRepo, ICityValidation validation)
        {
            _cityRepo = cityRepo;
            _Validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(EditCityCommand request, CancellationToken cancellationToken)
        {
            var validationresult = await _Validation.EditCityValidation(request.command);
            if (validationresult.IsError) return validationresult.Errors;

            var city = await _cityRepo.GetByIdAsync(request.command.Id);
            city.Edit(request.command.Title, request.command.Status);
            var result = await _cityRepo.SaveChangesAsync(cancellationToken);
            if(result==false)
            {
                        return Error.Failure(code: "City.UpdateFailed",description: "خطا در بروزرسانی شهر");
            }
            return Result.Success;
        }
    }

}
