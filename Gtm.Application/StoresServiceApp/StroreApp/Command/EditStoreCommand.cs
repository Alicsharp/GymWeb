using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.StoresServiceApp.StroreApp.Command
{
    public record EditStoreCommand(int id, string des):IRequest<ErrorOr<Success>>;
    
}
