using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.StateApp.Query
{
    public record GetStatesForAdminQuery : IRequest<ErrorOr<List<StateAdminQueryModel>>>;

    public class GetStatesForAdminQueryHandler : IRequestHandler<GetStatesForAdminQuery, ErrorOr<List<StateAdminQueryModel>>>
    {
        private readonly IStateRepo _stateRepository;

        public GetStatesForAdminQueryHandler(IStateRepo stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public async Task<ErrorOr<List<StateAdminQueryModel>>> Handle(GetStatesForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stateRepository.GetStatesForAdminAsync();

                // اگر لیست خالی باشد، خطا برگردانید
                if (result == null)
                {
                    return Error.NotFound(
                        code: "States.NotFound",
                        description: "خطا در دریافت اطلاعات وضعیت‌ها از سرور");
                }

                // اگر لیست خالی باشد، پیام مناسب برگردانید
                if (!result.Any())
                {
                    return Error.NotFound(
                        code: "States.Empty",
                        description: "هیچ وضعیتی برای نمایش یافت نشد");
                }

                // اعتبارسنجی آیتم‌های لیست (اختیاری)
                var invalidItems = result.Where(x => string.IsNullOrWhiteSpace(x.Title)).ToList();
                if (invalidItems.Any())
                {
                    return Error.Validation(
                        code: "States.InvalidData",
                        description: $"تعداد {invalidItems.Count} وضعیت دارای نام نامعتبر هستند");
                }

                return result;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Database.Error",
                    description: $"خطا در ارتباط با پایگاه داده: {ex.Message}");
            }
        }
    }
}
