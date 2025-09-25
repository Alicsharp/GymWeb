using ErrorOr;
using Gtm.Contract.EmailContract.SensEmailContract.Command;


namespace Gtm.Application.EmailServiceApp.SensEmailApp
{
    public interface IEmailValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateSendEmail dto);
    }
    public class EmailValidator : IEmailValidator
    {
        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateSendEmail dto)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add(Error.Validation("Email.TitleRequired", "عنوان ایمیل الزامی است."));

            if (dto.Title?.Length > 100)
                errors.Add(Error.Validation("Email.TitleTooLong", "عنوان ایمیل نباید بیشتر از 100 کاراکتر باشد."));

            if (string.IsNullOrWhiteSpace(dto.Text))
                errors.Add(Error.Validation("Email.TextRequired", "متن ایمیل الزامی است."));

            if (dto.Text?.Length > 1000)
                errors.Add(Error.Validation("Email.TextTooLong", "متن ایمیل نباید بیشتر از 1000 کاراکتر باشد."));

            return errors.Any() ? errors : Result.Success;
        }
    }
}
