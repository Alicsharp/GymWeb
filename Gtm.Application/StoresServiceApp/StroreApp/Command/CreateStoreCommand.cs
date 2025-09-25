using ErrorOr;
using Gtm.Application.StoresServiceApp.StoreProductApp;
using Gtm.Contract.StoresContract.StoreContract.Command;
using Gtm.Domain.StoresDomain.StoreAgg;
using Gtm.Domain.StoresDomain.StoreProductAgg;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.StoresServiceApp.StroreApp.Command
{
    public record CreateStoreCommand(int _userId, CreateStore command) : IRequest<ErrorOr<Success>>;
    public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, ErrorOr<Success>>
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreProductRepository _storeProductRepository;

        public CreateStoreCommandHandler(IStoreRepository storeRepository, IStoreProductRepository storeProductRepository)
        {
            _storeRepository = storeRepository;
            _storeProductRepository = storeProductRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
        {
            // 1️⃣ ایجاد Store
            var store = new Store(request._userId, request.command.SellerId, request.command.Description);
            await _storeRepository.AddAsync(store);

            // SaveChangesAsync اول برای پر شدن Id
            await _storeRepository.SaveChangesAsync(cancellationToken);

            // 2️⃣ ایجاد StoreProduct ها فقط اگر محصول وجود دارد
            if (request.command.Products != null && request.command.Products.Any())
            {
                var storeProducts = request.command.Products
                    .Select(p => new StoreProduct(store.Id, p.ProductSellId, p.Type, p.Count))
                    .ToList();

                await _storeProductRepository.CreateListAsync(storeProducts);

                // SaveChangesAsync دوم برای Productها
                await _storeRepository.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }


}
