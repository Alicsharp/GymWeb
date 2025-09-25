using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.PostServiceApp.UserPostApp.Query
{
    public record GetPostOrdersForUserPanelQuery(int pageId, int userId) : IRequest<ErrorOr<PostOrderUserPanelPaging>>;
    public class GetPostOrdersForUserPanelQueryHandler : IRequestHandler<GetPostOrdersForUserPanelQuery, ErrorOr<PostOrderUserPanelPaging>>
    {
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IPackageRepo _packageRepository;

        public GetPostOrdersForUserPanelQueryHandler(IPostOrderRepo postOrderRepository, IPackageRepo packageRepository)
        {
            _postOrderRepository = postOrderRepository;
            _packageRepository = packageRepository;
        }

        public async Task<ErrorOr<PostOrderUserPanelPaging>> Handle(GetPostOrdersForUserPanelQuery request, CancellationToken cancellationToken)
        {
            // کوئری خام
            var query = _postOrderRepository
                .QueryBy(b => b.UserId == request.userId)
                .OrderByDescending(o => o.Id);

            // ساخت مدل صفحه‌بندی
            var model = new PostOrderUserPanelPaging();
            model.GetData(query, request.pageId, 10, 1);

            // گرفتن سفارش‌ها با skip و take
            var ordersPage = query
                .Skip(model.Skip)
                .Take(model.Take)
                .ToList(); // اجرای کوئری در DB

            // گرفتن همه آی‌دی‌های پکیج برای fetch کردن یکجا
            var packageIds = ordersPage.Select(o => o.PackageId).Distinct().ToList();

            var packagesDict = (await _packageRepository
                .GetAllByQueryAsync(p => packageIds.Contains(p.Id), cancellationToken))
                .ToDictionary(p => p.Id, p => p); // تبدیل به دیکشنری برای دسترسی سریع

            model.Orders = ordersPage.Select(o =>
            {
                var package = packagesDict.GetValueOrDefault(o.PackageId);

                return new PostOrderUserPanelQueryModel
                {
                    Count = package?.Count ?? 0,
                    Date = o.CreateDate.ToPersainDate(),
                    Id = o.Id,
                    PackageId = o.PackageId,
                    PackageImage = package != null ? $"{FileDirectories.PackageImageDirectory400}{package.ImageName}" : "",
                    PackageTitle = package?.Title ?? "",
                    Price = o.Price,
                    Status = o.Status,
                    transactionId = o.TransactionId,
                    TransactionRef = "" // TODO: set transaction ref if needed
                };
            }).ToList();

            return model;
        }
    }
}
