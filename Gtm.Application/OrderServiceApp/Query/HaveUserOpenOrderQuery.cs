using ErrorOr;
using Gtm.Contract.OrderContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.OrderServiceApp.Query
{
    public record HaveUserOpenOrderQuery(int UserId) : IRequest<ErrorOr<bool>>;
    /// <summary>
    /// هندلر برای کوئری HaveUserOpenOrderQuery
    /// </summary>
    public class HaveUserOpenOrderQueryHandler : IRequestHandler<HaveUserOpenOrderQuery, ErrorOr<bool>>
    {
        private readonly IOrderRepository _orderRepository;

        public HaveUserOpenOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// اجرای منطق کوئری
        /// </summary>
        public async Task<ErrorOr<bool>> Handle(HaveUserOpenOrderQuery request, CancellationToken cancellationToken)
        {
            // 1. فراخوانی متد ریپازیتوری که 'bool' برمی‌گرداند
            bool exists = await _orderRepository.HaveUserOpenOrderAsync(request.UserId);

            // 2. بازگرداندن نتیجه 'bool'
            // کتابخانه ErrorOr به طور خودکار این را به عنوان یک نتیجه موفقیت‌آمیز 
            // (Success) که مقدار bool را در خود دارد، بسته‌بندی می‌کند.
            return exists;

            // نکته: در این حالت، یک نتیجه خطا (Error) فقط زمانی برگردانده می‌شود
            // که ریپازیتوری یک Exception پرتاب کند و یک Middleware 
            // آن را گرفته و به Error تبدیل کند.
            // ما "false" بودن را یک خطا در نظر نمی‌گیریم.
        }
      
      
    }
 }
