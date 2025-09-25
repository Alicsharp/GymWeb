
using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Command;
using Utility.Appliation.FileService;
using Utility.Appliation;


namespace Gtm.Application.SiteServiceApp.MenuApp
{
    public interface IMenuValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateMenu command);
        Task<ErrorOr<Success>> ValidateEditAsync(EditMenu command); 
        Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id);
        Task<ErrorOr<Success>> ValidateGetForAdminAsync(int? parentId);
        Task<ErrorOr<Success>> ValidateGetForCreateSubMenuAsync(int parentId);
        Task<ErrorOr<Success>> ValidateGetForCreateAsync(int parentId);
        Task<ErrorOr<Success>> ValidateGetForEditAsync(int id);
    }
    public class MenuValidator : IMenuValidator
    {
        private readonly IFileService _fileService;
        private readonly IMenuRepository _menuRepository;

        public MenuValidator(IFileService fileService, IMenuRepository menuRepository)
        {
            _fileService = fileService;
            _menuRepository = menuRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateMenu command)
        {
            var errors = new List<Error>();

            // Validate title
            if (string.IsNullOrWhiteSpace(command.Title))
                errors.Add(Error.Validation("Menu.TitleRequired", "عنوان منو الزامی است."));
            else if (command.Title.Length > 100)
                errors.Add(Error.Validation("Menu.TitleTooLong", "عنوان منو نباید بیشتر از 100 کاراکتر باشد."));

            // Validate image
            if (command.ImageFile != null)
            {
                if (string.IsNullOrWhiteSpace(command.ImageAlt))
                    errors.Add(Error.Validation("Menu.ImageAltRequired", "متن جایگزین تصویر الزامی است."));

                if (!command.ImageFile.IsImage())
                    errors.Add(Error.Validation("Menu.InvalidImage", "فایل باید یک تصویر معتبر باشد."));

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                    errors.Add(Error.Validation("Menu.ImageTooLarge", "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
            }

            // Validate URL if provided
            if (!string.IsNullOrWhiteSpace(command.Url) &&
                !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
                errors.Add(Error.Validation("Menu.InvalidUrl", "آدرس URL معتبر نیست."));

            // Validate number uniqueness if needed
            var isNumberExists = await _menuRepository.ExistsAsync(m => m.Number == command.Number);
            if (isNumberExists)
                errors.Add(Error.Conflict("Menu.DuplicateNumber", "شماره منو تکراری است."));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditAsync(EditMenu command)
        {
            var errors = new List<Error>();

            // Validate menu exists
            var menuExists = await _menuRepository.ExistsAsync(c=>c.Id==command.Id);
            if (!menuExists)
            {
                errors.Add(Error.NotFound("Menu.NotFound", "منو مورد نظر یافت نشد."));
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation("Menu.TitleRequired", "عنوان منو الزامی است."));
            }
            else if (command.Title.Length > 100)
            {
                errors.Add(Error.Validation("Menu.TitleTooLong", "عنوان منو نباید بیشتر از 100 کاراکتر باشد."));
            }

            // Validate image
            if (command.ImageFile != null)
            {
                if (string.IsNullOrWhiteSpace(command.ImageAlt))
                {
                    errors.Add(Error.Validation("Menu.ImageAltRequired", "متن جایگزین تصویر الزامی است."));
                }

                if (!command.ImageFile.IsImage())
                {
                    errors.Add(Error.Validation("Menu.InvalidImage", "فایل باید یک تصویر معتبر باشد."));
                }

                if (command.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    errors.Add(Error.Validation("Menu.ImageTooLarge", "حجم تصویر نباید بیشتر از 5 مگابایت باشد."));
                }
            }

            // Validate URL if provided
            if (!string.IsNullOrWhiteSpace(command.Url) && !Uri.TryCreate(command.Url, UriKind.Absolute, out _))
            {
                errors.Add(Error.Validation("Menu.InvalidUrl", "آدرس URL معتبر نیست."));
            }

            // Validate number uniqueness (excluding current menu)
            var isNumberExists = await _menuRepository.ExistsAsync(m =>
                m.Number == command.Number && m.Id != command.Id);
            if (isNumberExists)
            {
                errors.Add(Error.Conflict("Menu.DuplicateNumber", "شماره منو تکراری است."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateActivationChangeAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation("Menu.InvalidId", "شناسه منو معتبر نیست."));
            }
            else if (!await _menuRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound("Menu.NotFound", "منو با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForAdminAsync(int? parentId)
        {
            var errors = new List<Error>();

            if (parentId.HasValue && parentId.Value < 0)
            {
                errors.Add(Error.Validation("Menu.InvalidParentId", "شناسه والد معتبر نیست."));
            }

            if (parentId.HasValue && parentId.Value > 0)
            {
                var parentExists = await _menuRepository.ExistsAsync(c => c.Id==parentId.Value);
                if (!parentExists)
                {
                    errors.Add(Error.NotFound("Menu.ParentNotFound", "منوی والد با این شناسه یافت نشد."));
                }
            }

            return errors.Any() ? errors : Result.Success;
        }
       
        public async Task<ErrorOr<Success>> ValidateGetForCreateSubMenuAsync(int parentId)
        {
            var errors = new List<Error>();

            if (parentId <= 0)
            {
                errors.Add(Error.Validation("Menu.InvalidParentId", "شناسه والد معتبر نیست."));
            }
            else if (!await _menuRepository.ExistsAsync(c => c.ParentId == parentId))
            {
                errors.Add(Error.NotFound("Menu.ParentNotFound", "منوی والد با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForCreateAsync(int parentId)
        {
            var errors = new List<Error>();

            if (parentId <= 0)
            {
                errors.Add(Error.Validation("Menu.InvalidParentId", "شناسه والد معتبر نیست."));
            }
            else if (!await _menuRepository.ExistsAsync(c => c.ParentId == parentId))
            {
                errors.Add(Error.NotFound("Menu.ParentNotFound", "منوی والد با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditAsync(int id)
        {
            var errors = new List<Error>();

            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    "Menu.InvalidId",
                    "شناسه منو باید بزرگتر از صفر باشد."));
            }
            else if (!await _menuRepository.ExistsAsync(c=>c.Id==id))
            {
                errors.Add(Error.NotFound(
                    "Menu.NotFound",
                    "منو با این شناسه یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
