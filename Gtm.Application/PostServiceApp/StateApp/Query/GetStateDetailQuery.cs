using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Query;
using Gtm.Domain.PostDomain.StateAgg;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetStateDetailQuery(int Id) : IRequest<ErrorOr<StateDetailQueryModel>>;

    public class GetStateDetailQueryHandler : IRequestHandler<GetStateDetailQuery, ErrorOr<StateDetailQueryModel>>
    {
        private readonly IStateRepo _stateRepository;
        private readonly IStateValidation _validator;

        public GetStateDetailQueryHandler(IStateRepo stateRepository,IStateValidation validator)
        {
            _stateRepository = stateRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<StateDetailQueryModel>> Handle( GetStateDetailQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی شناسه
                var idValidation = await _validator.ValidateIdAsync(request.Id);
                if (idValidation.IsError)
                    return idValidation.Errors;

                // دریافت داده
                var state = await _stateRepository.GetByIdAsync(request.Id);

                // اعتبارسنجی موجودیت
                var stateValidation = await _validator.ValidateStateAsync(state);
                if (stateValidation.IsError)
                    return stateValidation.Errors;

                // دریافت داده‌های وابسته
                var states = await _stateRepository.GetAllAsync( );
                var cities = await _stateRepository.GetAllByQueryAsync(
                    c => c.Id == request.Id,
                    cancellationToken);

                // بررسی null بودن نتایج
                if (states == null)
                    return Error.NotFound("State.ListNotFound", "لیست استان‌ها یافت نشد");

                if (cities == null)
                    return Error.NotFound("City.ListNotFound", "لیست شهرها یافت نشد");

                // مپینگ نتیجه
                return new StateDetailQueryModel
                {
                    Id = state.Id,
                    Name = state.Title,
                    CloseStates = state.CloseStates  ,
                    States = states.Select(s => new StateForAddStateClosesQueryModel
                    {
                        Id = s.Id,
                        title = s.Title
                    }).ToList(),
                    Cities = cities.Select(c => new CityAdminQueryModel
                    {
                        Id = c.Id,
                        Title = c.Title,
                        CreationDate = c.CreateDate.ToPersianDate()
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                return Error.Unexpected("UnexpectedError", $"خطای غیرمنتظره: {ex.Message}");
            }
        }
    }
}
