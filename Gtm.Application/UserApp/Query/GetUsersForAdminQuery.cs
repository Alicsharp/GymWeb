using ErrorOr;
using Gtm.Contract.UserContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.UserApp.Query
{
    public record GetUsersForAdminQuery(int pageId, string filter, int take, UserOrderBySearch orderBy, UserStatusSearch status) : IRequest<ErrorOr<UsersForAdminPaging>>;
    public class GetUsersForAdminQueryHandler : IRequestHandler<GetUsersForAdminQuery, ErrorOr<UsersForAdminPaging>>
    {
        private readonly IUserRepo _userRepository;
        //private readonly IWalletRepository _walletRepository;
        //private readonly ITransactionRepository _transactionRepository;

        //public GetUsersForAdminQueryHandler(IUserRepository userRepository, IWalletRepository walletRepository, ITransactionRepository transactionRepository)
        //{
        //    _userRepository = userRepository;
        //    _walletRepository = walletRepository;
        //    _transactionRepository = transactionRepository;
        //}
        public GetUsersForAdminQueryHandler(IUserRepo userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<ErrorOr<UsersForAdminPaging>> Handle(GetUsersForAdminQuery request, CancellationToken cancellationToken)
        {
            var result =   _userRepository.GetAllQueryable().OrderByDescending(u => u.Id);
            if (!string.IsNullOrEmpty(request.filter))
                result = result.Where(r =>
                r.Mobile.Contains(request.filter.ToLower().Trim()) ||
                r.Email.ToLower().Trim().Contains(request.filter.ToLower().Trim()) ||
                r.FullName.ToLower().Trim().Contains(request.filter.ToLower().Trim())
                ).OrderByDescending(u => u.Id);

            switch (request.status)
            {
                case UserStatusSearch.حذف_شده_ها:
                    result = result.Where(r => r.IsDelete).OrderByDescending(u => u.Id);
                    break;
                case UserStatusSearch.کاربران_فعال:
                    result = result.Where(r => r.Active).OrderByDescending(u => u.Id);
                    break;
                case UserStatusSearch.کاربران_غیر_فعال:
                    result = result.Where(r => !r.Active).OrderByDescending(u => u.Id);
                    break;
                default:
                    break;
            }
            switch (request.orderBy)
            {
                case UserOrderBySearch.تاریخ_ثبت_از_اول_به_آخر:
                    result = result.OrderBy(u => u.Id);
                    break;
                default:
                    break;
            }
            UsersForAdminPaging model = new();
            model.GetData(result, request.pageId, request.take, 2);
            model.OrderBy = request.orderBy;
            model.Status = request.status;
            model.Filter = request.filter;
            model.Users = new();
            if (model.DataCount > 0)
                model.Users = result.Skip(model.Skip).Take(model.Take).
                    Select(u => new UserForAdminQueryModel
                    {
                        Active = u.Active,
                        WalletAmount = 0,
                        Creationdate = u.CreateDate.ToPersianDate(),
                        Delete = u.IsDelete,
                        Email = u.Email,
                        FullName = u.FullName,
                        Id = u.Id,
                        Mobile = u.Mobile,
                        OrderCount = 0,
                        OrderSum = 0,
                        TransactionSuccessCount = 0,
                        TransactionSuccessSum = 0
                    }).ToList();

            //if (model.Users.Count() > 0)
            //    model.Users.ForEach(async x =>
            //    {
            //        x.WalletAmount = await _walletRepository.GetWalletAmountAsync(x.Id);
            //        x.TransactionSuccessCount = _transactionRepository.GetAllByQuery(t => t.AuthorUserId == x.Id && t.Status == TransactionStatus.موفق).Count();
            //        x.TransactionSuccessSum = _transactionRepository.GetAllByQuery(t => t.AuthorUserId == x.Id && t.Status == TransactionStatus.موفق).Sum(t => t.Price);
            //    });

            return model;
        }
    }
}
