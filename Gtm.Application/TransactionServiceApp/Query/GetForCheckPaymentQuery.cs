using ErrorOr;
using Gtm.Contract.TransactionContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.TransactionServiceApp.Query
{
    public record GetForCheckPaymentQuery(string authority): IRequest<ErrorOr<TransactionQueryModel>>;
    public class GetForCheckPaymentQueryHandelr : IRequestHandler<GetForCheckPaymentQuery, ErrorOr<TransactionQueryModel>>
    {
        private readonly ITransactionRepository _transactionRepository;

        public GetForCheckPaymentQueryHandelr(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<ErrorOr<TransactionQueryModel>> Handle(GetForCheckPaymentQuery request, CancellationToken cancellationToken)
        {
            var tranaction = await _transactionRepository.GetByAuthorityAsync(request.authority);
            if (tranaction == null) return Error.Failure();
            return new TransactionQueryModel(tranaction.Id, tranaction.UserId, tranaction.Price, tranaction.RefId,
                tranaction.Portal, tranaction.Status, tranaction.TransactionFor, tranaction.OwnerId);
        }
    }
}
