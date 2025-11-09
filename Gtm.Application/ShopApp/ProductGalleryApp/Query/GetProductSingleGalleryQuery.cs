using ErrorOr;
using Gtm.Contract.ProductGalleryContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductGalleryApp.Query
{
    public record GetProductSingleGalleryQuery(int productId):IRequest<ErrorOr<List<GalleryForProductSingleQueryModel>>>;
    public class GetProductSingleGalleryQueryHandler : IRequestHandler<GetProductSingleGalleryQuery, ErrorOr<List<GalleryForProductSingleQueryModel>>>
    {
        private readonly IProductGalleryRepository _repository;

        public GetProductSingleGalleryQueryHandler(IProductGalleryRepository repository)
        {
            _repository = repository;
        }

        public async Task<ErrorOr<List<GalleryForProductSingleQueryModel>>> Handle(GetProductSingleGalleryQuery request, CancellationToken cancellationToken)
        {
           return await  _repository.GetProductSingleGalleryAsync(request.productId);
        }
    }
}
