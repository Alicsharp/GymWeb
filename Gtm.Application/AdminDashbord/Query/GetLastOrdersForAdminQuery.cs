using ErrorOr;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.UserApp;
using Gtm.Contract.AdminDashboard;
using MediatR;
using Utility.Appliation;
using static Gtm.Application.AdminDashbord.Query.GetAdminDataQueryHandler;

namespace Gtm.Application.AdminDashbord.Query
{
    public record GetLastOrdersForAdminQuery()
   
            : IRequest<ErrorOr<List<LastOrderAdminQueryModel>>>;
    public partial class GetLastOrdersForAdminQueryHandler
    : IRequestHandler<GetLastOrdersForAdminQuery, ErrorOr<List<LastOrderAdminQueryModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepo _userRepository;

        public GetLastOrdersForAdminQueryHandler(IOrderRepository orderRepository, IUserRepo userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<List<LastOrderAdminQueryModel>>> Handle(
            GetLastOrdersForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی ۱۰ سفارش (فقط ۱ کوئری)
            var lastOrders = await _orderRepository.GetLast10OrdersAsync(cancellationToken);

            if (lastOrders == null || !lastOrders.Any())
            {
                return new List<LastOrderAdminQueryModel>(); // بازگشت لیست خالی
            }

            // 2. مپ کردن اولیه (در حافظه)
            List<LastOrderAdminQueryModel> model = lastOrders.Select(o => new LastOrderAdminQueryModel
            {
                CreationDate = o.CreateDate.ToPersianDate(),
                OrderId = o.Id,
                PaymentPrice = o.PaymentPrice,
                Status = o.OrderStatus,
                UserId = o.UserId,
                UserName = "" // (در مرحله بعد پر می‌شود)
            }).ToList();

            // --- حل مشکل N+1 ---
            // 3. جمع‌آوری تمام شناسه‌های کاربری
            var userIds = model.Select(m => m.UserId).Distinct().ToList();

            // 4. واکشی تمام کاربران (فقط ۱ کوئری)
            var users = await _userRepository.GetUsersByIdsAsync(userIds, cancellationToken);
            var userDictionary = users.ToDictionary(u => u.Id);

            // 5. پر کردن نام کاربران (در حافظه، بدون کوئری اضافه)
            foreach (var item in model)
            {
                if (userDictionary.TryGetValue(item.UserId, out var user))
                {
                    item.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                }
                else
                {
                    item.UserName = "کاربر حذف شده"; // (مدیریت حالت نال)
                }
            }

            return model;
        }
    }
}
