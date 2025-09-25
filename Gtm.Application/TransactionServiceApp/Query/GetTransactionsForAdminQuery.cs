using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Application.UserApp;
using Gtm.Application.WalletServiceApp;
using Gtm.Contract.TransactionContract.Query;
using Gtm.Contract.WalletContract.Query;
using Gtm.Domain.TransactionDomian;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.TransactionServiceApp.Query
{
    public record GetTransactionsForAdminQuery(int pageId, int userId,string filter, int take, OrderingWalletSearch orderby,
    TransactionForSearch transactionFor, TransactionStatusSearch status, TransactionPortalSearch portal):IRequest<ErrorOr<TransactionsForAdminPaging>>;
    public class GetTransactionsForAdminQueryHandler : IRequestHandler<GetTransactionsForAdminQuery, ErrorOr<TransactionsForAdminPaging>>
    {
        private readonly IUserRepo _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;

        public GetTransactionsForAdminQueryHandler(IUserRepo userRepository, IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<ErrorOr<TransactionsForAdminPaging>> Handle(GetTransactionsForAdminQuery request, CancellationToken cancellationToken)
        {

            #region befrorcode
            //string title = "لیست تراکنش ها";
            //if (request.userId > 0)
            //{
            //    var user = await _userRepository.GetByIdAsync(request.userId);
            //    title = title + $" برای کاربر {user.FullName ?? ""} - {user.Mobile}";
            //}
            //IQueryable<Transaction> result = _transactionRepository.GetAllQueryable().OrderByDescending(o => o.Id);
            //if (request.userId > 0)
            //    result = result.Where(r => r.UserId == request.userId).OrderByDescending(o => o.Id);

            //if (!string.IsNullOrEmpty(request.filter))
            //    result = result.Where(r => r.RefId.Contains(request.filter)).OrderByDescending(o => o.Id);

            //switch (request.orderby)
            //{
            //    case OrderingWalletSearch.بر_اساس_تاریخ_از_اول:
            //        result = result.OrderBy(o => o.Id);
            //        break;
            //    case OrderingWalletSearch.بر_اساس_مبلغ_از_بالا_به_پایین:
            //        result = result.OrderByDescending(o => o.Price);
            //        break;
            //    case OrderingWalletSearch.بر_اساس_مبلغ_از_پایین_به_بالا:
            //        result = result.OrderBy(o => o.Price);
            //        break;
            //    default:
            //        break;
            //}
            //switch (request.transactionFor)
            //{
            //    case TransactionForSearch.کیف_پول:
            //        result = result.Where(r => r.TransactionFor == TransactionFor.Wallet);
            //        break;
            //    case TransactionForSearch.فاکتور:
            //        result = result.Where(r => r.TransactionFor == TransactionFor.Order);
            //        break;
            //    default:
            //        break;
            //}
            //switch (request.status)
            //{
            //    case TransactionStatusSearch.نا_موفق:
            //        result = result.Where(r => r.Status == TransactionStatus.نا_موفق);
            //        break;
            //    case TransactionStatusSearch.موفق:
            //        result = result.Where(r => r.Status == TransactionStatus.موفق);
            //        break;
            //    default:
            //        break;
            //}
            //switch (request.portal)
            //{
            //    case TransactionPortalSearch.زرین_پال:
            //        result = result.Where(r => r.Portal == TransactionPortal.زرین_پال);
            //        break;
            //    default:
            //        break;
            //}
            //TransactionsForAdminPaging model = new();
            //model.GetData(result,request.pageId,request.take, 2);
            //model.Status = request.status;
            //model.Transactions = new();
            //model.TransactiionSuccessSum = 0;
            //model.Filter = request.filter;
            //model.OrderBy = request.orderby;
            //model.PageTitle = title;
            //model.UserId =request.userId;
            //model.TransactionFor = request.transactionFor;
            //model.Portal = request.portal;
            //if (model.DataCount > 0)
            //    model.Transactions = result.Skip(model.Skip).Take(model.Take).
            //        Select(t => new TransactionForAdminQueryModel
            //        {
            //            CretionDate = t.CreateDate.ToPersainDate(),
            //            Id = t.Id,
            //            OwnerId = t.OwnerId,
            //            Portal = t.Portal,
            //            Price = t.Price,
            //            RefId = t.RefId,
            //            Status = t.Status,
            //            TransactionFor = t.TransactionFor,
            //            UserId = t.UserId,
            //            UserName = ""
            //        }).ToList();

            //model.Transactions.ForEach(async x =>
            //{
            //    var user =await _userRepository.GetByIdAsync(x.UserId);
            //    x.UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
            //});
            //return model;
            #endregion
            string title = "لیست تراکنش ها";

            // اضافه کردن نام کاربر به عنوان صفحه اگر userId داده شده
            if (request.userId > 0)
            {
                var user = await _userRepository.GetByIdAsync(request.userId);
                title += $" برای کاربر {user.FullName ?? ""} - {user.Mobile}";
            }

            // کوئری پایه
            IQueryable<Transaction> result = _transactionRepository
                .GetAllQueryable()
                .OrderByDescending(o => o.Id);

            // فیلترها
            if (request.userId > 0)
                result = result.Where(r => r.UserId == request.userId);

            if (!string.IsNullOrEmpty(request.filter))
                result = result.Where(r => r.RefId.Contains(request.filter));

            // مرتب‌سازی
            result = request.orderby switch
            {
                OrderingWalletSearch.بر_اساس_تاریخ_از_اول => result.OrderBy(o => o.Id),
                OrderingWalletSearch.بر_اساس_مبلغ_از_بالا_به_پایین => result.OrderByDescending(o => o.Price),
                OrderingWalletSearch.بر_اساس_مبلغ_از_پایین_به_بالا => result.OrderBy(o => o.Price),
                _ => result.OrderByDescending(o => o.Id)
            };

            // فیلتر بر اساس TransactionFor
            if (request.transactionFor == TransactionForSearch.کیف_پول)
                result = result.Where(r => r.TransactionFor == TransactionFor.Wallet);
            else if (request.transactionFor == TransactionForSearch.فاکتور)
                result = result.Where(r => r.TransactionFor == TransactionFor.Order);

            // فیلتر بر اساس Status
            if (request.status == TransactionStatusSearch.نا_موفق)
                result = result.Where(r => r.Status == TransactionStatus.نا_موفق);
            else if (request.status == TransactionStatusSearch.موفق)
                result = result.Where(r => r.Status == TransactionStatus.موفق);

            // فیلتر بر اساس Portal
            if (request.portal == TransactionPortalSearch.زرین_پال)
                result = result.Where(r => r.Portal == TransactionPortal.زرین_پال);

            // ساخت مدل صفحه‌بندی
            TransactionsForAdminPaging model = new();
            model.GetData(result, request.pageId, request.take, 2);
            model.Status = request.status;
            model.Transactions = new();
            model.TransactiionSuccessSum = 0;
            model.Filter = request.filter;
            model.OrderBy = request.orderby;
            model.PageTitle = title;
            model.UserId = request.userId;
            model.TransactionFor = request.transactionFor;
            model.Portal = request.portal;

            // گرفتن دیتا
            if (model.DataCount > 0)
            {
                model.Transactions = await result
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .Select(t => new TransactionForAdminQueryModel
                    {
                        CretionDate = t.CreateDate.ToPersainDate(),
                        Id = t.Id,
                        OwnerId = t.OwnerId,
                        Portal = t.Portal,
                        Price = t.Price,
                        RefId = t.RefId,
                        Status = t.Status,
                        TransactionFor = t.TransactionFor,
                        UserId = t.UserId,
                        UserName = "" // بعداً پر می‌کنیم
                    })
                    .ToListAsync(cancellationToken);

                // بهینه: گرفتن همه‌ی کاربران یکجا به جای صدا زدن async در foreach
                var userIds = model.Transactions.Select(x => x.UserId).Distinct().ToList();
                var users = await _userRepository.GetByIdsAsync(userIds); // متد جدید که باید بسازی

                foreach (var tx in model.Transactions)
                {
                    var user = users.FirstOrDefault(u => u.Id == tx.UserId);
                    tx.UserName = string.IsNullOrEmpty(user?.FullName) ? user?.Mobile ?? "" : user.FullName;
                }
            }

            return model;

        }


    }
}
