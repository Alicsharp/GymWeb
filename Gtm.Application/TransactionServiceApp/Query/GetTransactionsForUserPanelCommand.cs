using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Application.WalletServiceApp;
using Gtm.Contract.TransactionContract.Query;
using Gtm.Domain.TransactionDomian;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.TransactionServiceApp.Query
{
      
    public record GetTransactionsForUserPanelCommand(int UserId, int PageId, string Filter): IRequest<ErrorOr<TransactionUserPanelPaging>>;

    public class GetTransactionsForUserPanelCommandHandler
        : IRequestHandler<GetTransactionsForUserPanelCommand, ErrorOr<TransactionUserPanelPaging>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepo _userRepository;
        private readonly ITransactionValidator _validator;

        public GetTransactionsForUserPanelCommandHandler(ITransactionRepository transactionRepository, IWalletRepository walletRepository, IUserRepo userRepository, ITransactionValidator validator)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _userRepository = userRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<TransactionUserPanelPaging>> Handle(GetTransactionsForUserPanelCommand request,CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی دسترسی کاربر
            var validationResult = await _validator.ValidateUserAccessAsync(request.UserId);
            if (validationResult.IsError)
                return validationResult.Errors;

            // 2. کوئری پایه
            IQueryable<Transaction> result = _transactionRepository
                .QueryBy(t => t.UserId == request.UserId)
                .OrderByDescending(o => o.Id);

            // 3. اعمال فیلتر
            if (!string.IsNullOrEmpty(request.Filter))
            {
                result = result
                    .Where(r => r.RefId.Contains(request.Filter))
                    .OrderByDescending(r => r.Id);
            }

            // 4. مدل پیجینگ
            TransactionUserPanelPaging model = new();
            model.GetData(result, request.PageId, 15, 2); // 15 = PageSize, 2 = ShowPageCount
            model.Filter = request.Filter;
            model.UserId = request.UserId;

            // 5. گرفتن داده‌ها
            if (await result.AnyAsync(cancellationToken))
            {
                model.Transaction = await result
                    .Skip(model.Skip)   
                    .Take(model.Take)
                    .Select(t => new TransactionUserPanelQueryModel
                    {
                        CretionDate = t.CreateDate.ToPersianDate(),
                        Id = t.Id,
                        OwnerId = t.OwnerId,
                        Portal = t.Portal,
                        Price = t.Price,
                        RefId = t.RefId,
                        Status = t.Status,
                        TransactionFor = t.TransactionFor
                    })
                    .ToListAsync(cancellationToken);
            }
            else
            {
                model.Transaction = new();
            }

            // 6. موجودی کیف پول و نام کاربر
            model.WalletAmount =   _walletRepository.GetWalletAmount(model.UserId);
            var user = await _userRepository.GetByIdAsync(request.UserId);
            model.FullName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;

            return model;
        }


    }
}
