
using ErrorOr;
using Gtm.Contract.PostContract.StateContract.Command;
using Gtm.Domain.PostDomain.StateAgg;

namespace Gtm.Application.PostServiceApp.StateApp
{
    public interface IStateValidation
    {
        ErrorOr<Success> ValidateStateCloseRequest(int stateId, List<int> closeStateIds);
        Task<ErrorOr<Success>> ValidateStateExists(int stateId);
        Task<ErrorOr<Success>> ValidateCreateStateAsync(CreateStateModel model);
        Task<ErrorOr<Success>> ValidateEditStateAsync(EditStateModel model);
        Task<ErrorOr<Success>> ValidateStateTitleAsync(string title);
        Task<ErrorOr<Success>> ValidateIdAsync(int id);
        Task<ErrorOr<Success>> ValidateStateAsync(State state);
        Task<ErrorOr<Success>> ValidateStateIdAsync(int id);
        Task<ErrorOr<Success>> ValidateStateForEditAsync(EditStateModel model);
        

    }
    public class StateValidation : IStateValidation
    {
        private readonly IStateRepo _stateRepository;

        public StateValidation(IStateRepo stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public ErrorOr<Success> ValidateStateCloseRequest(int stateId, List<int> closeStateIds)
        {
            var errors = new List<Error>();

            if (stateId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "State.InvalidId",
                    description: "شناسه استان نامعتبر است"));
            }

            if (closeStateIds == null || !closeStateIds.Any())
            {
                errors.Add(Error.Validation(
                    code: "State.CloseStatesEmpty",
                    description: "لیست استان‌های نزدیک نمی‌تواند خالی باشد"));
            }

            if (closeStateIds?.Any(x => x <= 0) == true)
            {
                errors.Add(Error.Validation(
                    code: "State.InvalidCloseStateId",
                    description: "شناسه استان‌های نزدیک نامعتبر است"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateStateExists(int stateId)
        {
            var stateExists = await _stateRepository.ExistsAsync(s => s.Id == stateId);
            return stateExists
                ? Result.Success
                : Error.NotFound(
                    code: "State.NotFound",
                    description: $"استان با شناسه {stateId} یافت نشد");
        }
        public async Task<ErrorOr<Success>> ValidateCreateStateAsync(CreateStateModel model)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                errors.Add(Error.Validation(
                    code: "State.InvalidTitle",
                    description: "عنوان استان نمی‌تواند خالی باشد"));
            }
            else if (model.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    code: "State.TitleTooLong",
                    description: "عنوان استان نمی‌تواند بیش از 100 کاراکتر باشد"));
            }

            // اعتبارسنجی تکراری نبودن عنوان (به صورت ناهمزمان)
            var titleExists = await _stateRepository.ExistsAsync(s => s.Title == model.Title);
            if (titleExists)
            {
                errors.Add(Error.Conflict(
                    code: "State.DuplicateTitle",
                    description: "استانی با این عنوان قبلاً ثبت شده است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditStateAsync(EditStateModel model)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                errors.Add(Error.Validation(
                    code: "State.InvalidTitle",
                    description: "عنوان استان نمی‌تواند خالی باشد"));
            }
            else if (model.Title.Length > 100)
            {
                errors.Add(Error.Validation(
                    code: "State.TitleTooLong",
                    description: "عنوان استان نمی‌تواند بیش از 100 کاراکتر باشد"));
            }

            // بررسی وجود استان
            var stateExists = await _stateRepository.ExistsAsync(s => s.Id == model.Id);
            if (!stateExists)
            {
                errors.Add(Error.NotFound(
                    code: "State.NotFound",
                    description: "استان مورد نظر یافت نشد"));
            }

            // بررسی تکراری نبودن عنوان (به جز برای خود استان)
            var titleExists = await _stateRepository.ExistsAsync(s =>
                s.Title == model.Title && s.Id != model.Id);
            if (titleExists)
            {
                errors.Add(Error.Conflict(
                    code: "State.DuplicateTitle",
                    description: "استانی با این عنوان قبلاً ثبت شده است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public ErrorOr<Success> ValidateStateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Error.Validation(
                    code: "State.EmptyTitle",
                    description: "عنوان استان نمی‌تواند خالی باشد");
            }

            if (title.Length > 100)
            {
                return Error.Validation(
                    code: "State.TitleTooLong",
                    description: "عنوان استان نمی‌تواند بیش از 100 کاراکتر باشد");
            }

            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateStateTitleAsync(string title)
        {
            // اعتبارسنجی ساده
            if (string.IsNullOrWhiteSpace(title))
                return Error.Validation("State.EmptyTitle", "...");

            // اعتبارسنجی ناهمزمان وجود عنوان در دیتابیس
            var exists = await _stateRepository.ExistsAsync(s => s.Title == title);
            if (exists)
                return Error.Conflict("State.DuplicateTitle", "...");

            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateStateIdAsync(int id)
        {
            if (id <= 0)
                return Error.Validation("State.InvalidId", "شناسه استان باید بزرگتر از صفر باشد");

            var exists = await _stateRepository.ExistsAsync(c => c.Id == id);
            return exists
                ? Result.Success
                : Error.NotFound("State.NotExists", "استان مورد نظر وجود ندارد");
        }

        public async Task<ErrorOr<Success>> ValidateStateExistsAsync(int stateId)
        {
            var state = await _stateRepository.ExistsAsync(c=>c.Id==stateId);

            if (!state)
            {
                
                return Error.NotFound(
                    code: "State.NotFound",
                    description: "استان مورد نظر یافت نشد");
            }

            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateIdAsync(int id)
        {
            if (id <= 0)
                return Error.Validation("State.InvalidId", "شناسه استان نامعتبر است");

            return Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateStateAsync(State state)
        {
            if (state == null)
                return Error.NotFound("State.NotFound", "استان مورد نظر یافت نشد");

            // اعتبارسنجی اضافی در صورت نیاز
            return Result.Success;
        }
      

        public async Task<ErrorOr<Success>> ValidateStateForEditAsync(EditStateModel model)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(model.Title))
                errors.Add(Error.Validation("State.EmptyTitle", "عنوان استان نمی‌تواند خالی باشد"));

            if (model.Title?.Length > 100)
                errors.Add(Error.Validation("State.TitleTooLong", "عنوان استان نمی‌تواند بیش از 100 کاراکتر باشد"));

            

            return errors.Any() ? errors : Result.Success;
        }
    }
}
