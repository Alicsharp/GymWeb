using ErrorOr;
using MediatR;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record ExistTitleForEditQuery(string Title, int Id, int StateId) : IRequest<ErrorOr<Success>>;
    public class ExistTitleForEditQueryHandler : IRequestHandler<ExistTitleForEditQuery, ErrorOr<Success>>
    {
        private readonly ICityRepo _cityRepo;
        private readonly ICityValidation _cityValidatio;

        public ExistTitleForEditQueryHandler(ICityRepo cityRepository, ICityValidation cityValidatio)
        {
            _cityRepo = cityRepository;
            _cityValidatio = cityValidatio;
        }

        public async Task<ErrorOr<Success>> Handle(ExistTitleForEditQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _cityValidatio.ExistTitleForCreateCityValidation(request.Title, request.StateId);
            if (validationResult.IsError) return validationResult.Errors;

             var result= await _cityRepo.ExistsAsync(c => c.Title == request.Title && c.StateId == request.StateId && c.Id != request.Id);
             if(result==false) return Result.Success;

            return Error.Failure("ExsistedTitle", "این عنوان  قبلا ثبت شده است");
        }
    }
}
