using ErrorOr;
using Gtm.Application.ArticleApp;
using Gtm.Application.CommentApp;
using Gtm.Application.OrderServiceApp;
using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.ProductSellApp;
using Gtm.Application.ShopApp.ProductVisitApp;
using Gtm.Application.TransactionServiceApp;
using Gtm.Application.UserApp;
using Gtm.Contract.AdminDashboard;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.AdminDashbord.Query
{
    /// <summary>
    /// کوئری برای دریافت داده‌های تجمعی داشبورد ادمین
    /// </summary>
    public record GetAdminDataQuery() : IRequest<ErrorOr<AdminDataQueryModel>>;
   public partial class GetAdminDataQueryHandler: IRequestHandler<GetAdminDataQuery, ErrorOr<AdminDataQueryModel>>
    {
        // تزریق تمام ریپازیتوری‌های اختصاصی
        private readonly IArticleRepo _articleRepo;
        private readonly ICommentRepo _commentRepo;
        private readonly IOrderRepository _orderRepository;
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductVisitRepository _productVisitRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepo _userRepository;

        public GetAdminDataQueryHandler(IArticleRepo articleRepo,
            ICommentRepo commentRepo,
            IOrderRepository orderRepository,
            IPostOrderRepo postOrderRepository,
            IProductRepository productRepository, 
            IProductSellRepository productSellRepository, 
            IProductVisitRepository productVisitRepository,
            ITransactionRepository transactionRepository, 
            IUserRepo userRepository)
        {
            _articleRepo = articleRepo;
            _commentRepo = commentRepo;
            _orderRepository = orderRepository;
            _postOrderRepository = postOrderRepository;
            _productRepository = productRepository;
            _productSellRepository = productSellRepository;
            _productVisitRepository = productVisitRepository;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

            public async Task<ErrorOr<AdminDataQueryModel>> Handle(
                GetAdminDataQuery request, CancellationToken cancellationToken)
            {
                // --- تعریف بازه‌های زمانی ---
                var now = DateTime.Now;
                var oneDayAgo = now.AddDays(-1);
                var oneWeekAgo = now.AddDays(-7);
                var oneMonthAgo = now.AddMonths(-1);

                // --- اجرای کوئری‌ها به صورت ترتیبی (Sequential) ---
                // (ما Task.WhenAll را حذف کردیم و هر خط را مستقیماً await می‌کنیم)

                // Blogs (Articles)
                int blogCount = await _articleRepo.CountAsync();
                int blogCountComment = await _commentRepo.CountAsync(c => c.CommentFor == CommentFor.مقاله);
                int blogCountVisit = await _articleRepo.GetTotalVisitCountAsync(cancellationToken);

                // Orders
                int orderCount = await _orderRepository.CountAsync(c => c.OrderStatus == OrderStatus.پرداخت_شده || c.OrderStatus == OrderStatus.ارسال_شده);
                int orderItemCount = await _orderRepository.GetActiveOrderItemCountAsync(cancellationToken);
                int orderSellerCount = await _orderRepository.GetActiveOrderSellerCountAsync(cancellationToken);
                int orderPostCount = await _postOrderRepository.CountAsync(p => p.Status == PostOrderStatus.پرداخت_شده);

                // Products
                int productCount = await _productRepository.CountAsync();
                int productCountComment = await _commentRepo.CountAsync(c => c.CommentFor == CommentFor.محصول);
                int productCountSell = await _productSellRepository.CountAsync();
                int productCountVisit = await _productVisitRepository.GetTotalVisitCountAsync(cancellationToken);

                // Transactions
                int transactionCount = await _transactionRepository.CountAsync(t => t.Status == TransactionStatus.موفق);
                int transactionSum = await _transactionRepository.GetSuccessfulTransactionSumAsync(cancellationToken);

                // Users
                int userCount = await _userRepository.CountAsync();
                int userCountDay = await _userRepository.CountAsync(u => u.CreateDate >= oneDayAgo);
                int userCountMounth = await _userRepository.CountAsync(u => u.CreateDate >= oneMonthAgo);
                int userCountWeek = await _userRepository.CountAsync(u => u.CreateDate >= oneWeekAgo);

                // --- (بلوک Task.WhenAll حذف شد) ---

                // --- ساخت مدل نهایی با نتایج ---
                AdminDataQueryModel model = new()
                {
                    BlogCount = blogCount,
                    BlogCountComment = blogCountComment,
                    BlogCountVisit = blogCountVisit,
                    OrderCount = orderCount,
                    OrderItemCount = orderItemCount,
                    OrderPostCount = orderPostCount,
                    OrderSellerCount = orderSellerCount,
                    ProductCount = productCount,
                    ProductCountComment = productCountComment,
                    ProductCountSell = productCountSell,
                    ProductCountVisit = productCountVisit,
                    TransactionCount = transactionCount,
                    TransactionSum = transactionSum,
                    UserCount = userCount,
                    UserCountDay = userCountDay,
                    UserCountMounth = userCountMounth,
                    UserCountWeek = userCountWeek
                };

                return model;
            }
        }
}
 
