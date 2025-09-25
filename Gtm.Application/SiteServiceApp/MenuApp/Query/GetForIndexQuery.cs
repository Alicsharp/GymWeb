using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetForIndexQuery : IRequest<ErrorOr<List<MenuForUi>>>;
    public class GetForIndexQueryHandler : IRequestHandler<GetForIndexQuery, ErrorOr<List<MenuForUi>>>
    {
        private readonly IMenuRepository _menuRepository;

        public GetForIndexQueryHandler(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<ErrorOr<List<MenuForUi>>> Handle(GetForIndexQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get active main menus
                var mainMenusQuery = _menuRepository.GetAllQueryable()
                    .Where(m => m.Active &&
                        (m.Status == MenuStatus.منوی_اصلی ||
                         m.Status == MenuStatus.منوی_اصلی_با_زیر_منو));

                // Project to MenuForUi and execute query
                var model = await mainMenusQuery
                    .Select(m => new MenuForUi
                    {
                        Id = m.Id,
                        Number = m.Number,
                        Title = m.Title,
                        Url = m.Url,
                        ImageAlt = m.ImageAlt,
                        ImageName = string.IsNullOrEmpty(m.ImageName)
                            ? ""
                            : FileDirectories.MenuImageDirectory + m.ImageName,
                        Status = m.Status,
                        Childs = new List<MenuForUi>()
                    })
                    .ToListAsync(cancellationToken);

                // Load first level children
                foreach (var menu in model)
                {
                    if (await _menuRepository.ExistsAsync(m =>
                        m.ParentId == menu.Id && m.Active))
                    {
                        menu.Childs = await _menuRepository.GetAllQueryable()
                            .Where(m => m.Active && m.ParentId == menu.Id)
                            .Select(m => new MenuForUi
                            {
                                Id = m.Id,
                                Number = m.Number,
                                Title = m.Title,
                                Url = m.Url,
                                Status = m.Status,
                                Childs = new List<MenuForUi>()
                            })
                            .ToListAsync(cancellationToken);
                    }
                }

                // Load second level children for menus with submenus
                foreach (var menu in model.Where(m =>
                    m.Status == MenuStatus.منوی_اصلی_با_زیر_منو &&
                    m.Childs.Any()))
                {
                    foreach (var subMenu in menu.Childs)
                    {
                        subMenu.Childs = await _menuRepository.GetAllQueryable()
                            .Where(m => m.Active && m.ParentId == subMenu.Id)
                            .Select(m => new MenuForUi
                            {
                                Id = m.Id,
                                Number = m.Number,
                                Title = m.Title,
                                Url = m.Url,
                                Status = m.Status
                            })
                            .ToListAsync(cancellationToken);
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    "Menu.RetrievalError",
                    $"خطا در دریافت لیست منوها: {ex.Message}");
            }
        }
    }
}
