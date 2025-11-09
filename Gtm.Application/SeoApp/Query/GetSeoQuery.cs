using ErrorOr;
using Gtm.Contract.SeoContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.SeoApp.Query
{
    public record GetSeoQuery(int ownerId, WhereSeo where, string title) : IRequest<ErrorOr<SeoQueryModel>>;
    public class GetSeoQueryHandler : IRequestHandler<GetSeoQuery, ErrorOr<SeoQueryModel>>
    {
        private readonly ISeoRepository _repository;

        public GetSeoQueryHandler(ISeoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ErrorOr<SeoQueryModel>> Handle(GetSeoQuery request, CancellationToken cancellationToken)
        {
            var seo = await _repository.GetSeoForUiAsync(request.ownerId, request.where, request.title);

            // 1. (راه‌حل خوب): ابتدا بررسی کنید که آیا نتیجه‌ای یافت شده؟
            if (seo == null)
            {
                // 2. اگر یافت نشد، یک "Error" برگردانید
                return Error.NotFound(description: "اطلاعات سئو یافت نشد.");
            }

            // 3. اگر یافت شد، *خودِ مدل* را بسازید
            var seoModel = new SeoQueryModel(
                seo.MetaTitle,
                seo.MetaDescription,
                seo.MetaKeyWords,
                seo.IndexPage,
                seo.Canonical,
                seo.Schema
            );

            // 4. خودِ مدل را return کنید (ErrorOr بقیه کار را انجام می‌دهد)
            return seoModel;
        }
    }
}
