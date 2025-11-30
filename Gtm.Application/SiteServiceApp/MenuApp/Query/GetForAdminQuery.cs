using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Query;
using Gtm.Domain.SiteDomain.MenuAgg;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;


namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetForAdminQuery(int? ParentId) : IRequest<ErrorOr<MenuPageAdminQueryModel>>;

    public class GetForAdminQueryHandler : IRequestHandler<GetForAdminQuery, ErrorOr<MenuPageAdminQueryModel>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuValidator _validator;

        public GetForAdminQueryHandler(IMenuRepository menuRepository,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<MenuPageAdminQueryModel>> Handle(GetForAdminQuery request,CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForAdminAsync(request.ParentId.Value);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                var model = new MenuPageAdminQueryModel();

                // Create base query
                IQueryable<Menu> query = _menuRepository.GetAllQueryable();

                if (!request.ParentId.HasValue || request.ParentId == 0)
                {
                    model.PageTitle = "لیست منوهای سردسته";
                    query = query.Where(m => m.ParentId == null);
                }
                else
                {
                    var menuParent = await _menuRepository.GetByIdAsync(request.ParentId.Value);
                    model.PageTitle = $"لیست زیر منو های {menuParent.Title} - وضعیت {menuParent.Status.ToString().Replace("_", " ")}";
                    model.Status = menuParent.Status;
                    model.Id = request.ParentId.Value;
                    query = query.Where(m => m.ParentId == request.ParentId.Value);
                }

                // Get menus with projection
                model.Menus = await query
                    .Select(m => new MenuForAdminQueryModel
                    {
                        Active = m.Active,
                        CreationDate = m.CreateDate.ToPersianDate(),
                        Id = m.Id,
                        Number = m.Number,
                        Status = m.Status,
                        Title = m.Title,
                        Url = m.Url,
                        ImageName = m.ImageName
                    })
                    .ToListAsync(cancellationToken);

                return model;
            }
            catch (Exception ex)
            {
                return Error.Failure("Menu.QueryFailed", $"خطا در دریافت لیست منوها: {ex.Message}");
            }
        }
    }


}
