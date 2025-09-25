using Gtm.Application.PostServiceApp.PostCalculateApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Query;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Contract.PostContract.StateContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gtm.WebApp.Controllers.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShippingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<List<StateQueryModel>> Get()
        {
            var state = await _mediator.Send(new GetStatesWithCityQuery());
            return state.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Get(PostPriceRequestApiModel command)
        {
            var model = await _mediator.Send(new PostCalculateApiCommand(command));
            if (model.IsError != false) { }

            return Ok(model.Value);
        }
    }
}
