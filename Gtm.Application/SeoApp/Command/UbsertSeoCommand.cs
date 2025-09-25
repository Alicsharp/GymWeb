using ErrorOr;
using Gtm.Contract.SeoContract.Command;
using Gtm.Domain.SeoDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SeoApp.Command
{
    public record UbsertSeoCommand(CreateSeo Command) : IRequest<ErrorOr<Success>>;

    public class UbsertSeoCommandHandler  : IRequestHandler<UbsertSeoCommand, ErrorOr<Success>>
    {
        private readonly ISeoRepository _repo;
        private readonly ISeoValidator _validator;

        public UbsertSeoCommandHandler(ISeoRepository repo, ISeoValidator validator)
        {
            _repo = repo;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(UbsertSeoCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetSeoAsync(request.Command.OwnerId, request.Command.Where);

            if (existing is null)
            {
                var seo = new Seo(
                    request.Command.MetaTitle,
                    request.Command.MetaDescription,
                    request.Command.MetaKeyWords,
                    request.Command.IndexPage,
                    request.Command.Canonical,
                    request.Command.Schema,
                    request.Command.Where,
                    request.Command.OwnerId);

               await _repo.AddAsync(seo);
                var Createdresult = await _repo.SaveChangesAsync(cancellationToken);
                if(Createdresult == true)
                {
                    return Result.Success;
                     
                }
                return Error.Failure("Seo.CreateFailed", " شکست هنگام ساخت");
            }

            existing.Edit(
                request.Command.MetaTitle,
                request.Command.MetaDescription,
                request.Command.MetaKeyWords,
                request.Command.IndexPage,
                request.Command.Canonical,
                request.Command.Schema);

            var Editresult = await _repo.SaveChangesAsync(cancellationToken);
            if (Editresult == true)
            {
                return Result.Success;

            }
            return Error.Failure("Seo.UpdateFailed", "شکست هنگام ویرایش");


        }
    }
}
