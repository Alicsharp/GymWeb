
using ErrorOr;
using Gtm.Application.SeoApp.Query;
using Gtm.Contract.SeoContract.Command;

namespace Gtm.Application.SeoApp
{
    public interface ISeoValidator
    {
        Task<ErrorOr<Success>> ValidateSeoData(CreateSeo command);
        Task<ErrorOr<Success>> ValidateGetSeoForEdit(GetSeoForEditCommand command);
    }
    public class SeoValidator : ISeoValidator
    {
        private readonly ISeoRepository _seoRepository;

        public SeoValidator(ISeoRepository seoRepository)
        {
            _seoRepository = seoRepository;
        }

        public async Task<ErrorOr<Success>> ValidateSeoData(CreateSeo command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان متا
            if (string.IsNullOrWhiteSpace(command.MetaTitle))
                errors.Add(Error.Validation("Seo.MetaTitleRequired", "عنوان متا الزامی است."));
            else if (command.MetaTitle.Length > 60)
                errors.Add(Error.Validation("Seo.MetaTitleTooLong", "عنوان متا نباید بیشتر از 60 کاراکتر باشد."));

            // اعتبارسنجی توضیحات متا
            if (string.IsNullOrWhiteSpace(command.MetaDescription))
                errors.Add(Error.Validation("Seo.MetaDescriptionRequired", "توضیحات متا الزامی است."));
            else if (command.MetaDescription.Length > 160)
                errors.Add(Error.Validation("Seo.MetaDescriptionTooLong", "توضیحات متا نباید بیشتر از 160 کاراکتر باشد."));

            // اعتبارسنجی OwnerId
            if (command.OwnerId <= 0)
                errors.Add(Error.Validation("Seo.InvalidOwnerId", "شناسه مالک نامعتبر است."));

            // اعتبارسنجی محل استفاده (Where)
            if (command.Where == null)
                errors.Add(Error.Validation("Seo.LocationRequired", "محل استفاده SEO الزامی است."));

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetSeoForEdit(GetSeoForEditCommand command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی OwnerId
            if (command.ownerId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Seo.InvalidOwnerId",
                    description: "شناسه مالک باید بزرگتر از صفر باشد"));
            }

            // اعتبارسنجی محل استفاده (Where)
            if (command.where == null)
            {
                errors.Add(Error.Validation(
                    code: "Seo.MissingLocation",
                    description: "محل استفاده SEO باید مشخص شود"));
            }


            return errors.Count > 0 ? errors : Result.Success;
        }
    }

}
