using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Command;
using MediatR;
 

namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetMenuForEditQuery(int Id) : IRequest<ErrorOr<EditMenu>>;

    public class GetMenuForEditQueryHandler : IRequestHandler<GetMenuForEditQuery, ErrorOr<EditMenu>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuValidator _validator;

        public GetMenuForEditQueryHandler(IMenuRepository menuRepository,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<EditMenu>> Handle(
            GetMenuForEditQuery request,
            CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForEditAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get menu for edit
                var menu = await _menuRepository.GetForEditAsync(request.Id);

                // Additional business validation
                if (menu == null)
                {
                    return Error.NotFound(
                        "Menu.NotFound",
                        "منو با این شناسه یافت نشد.");
                }

                return menu;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "Menu.RetrievalError",
                    $"خطا در دریافت اطلاعات منو: {ex.Message}");
            }
        }
    }
}
