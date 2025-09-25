using Gtm.Application.PostServiceApp.CityApp.Query;
using Gtm.Application.PostServiceApp.PackageApp.Query;
using Gtm.Application.PostServiceApp.PostCalculateApp.Command;
using Gtm.Application.PostServiceApp.StateApp.Query;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Gtm.WebApp.Controllers
{
    public class PostController : Controller
    {
        private readonly IMediator _mediator;
        public PostController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task<IActionResult> Index()
        {
            var model = await _mediator.Send(new GetPackagesForUiQuery());

            return View(model.Value);
        }
        public async Task<JsonResult> GetStates()
        {
            var model = await _mediator.Send(new GetStatesForChooseQuery());
            var json = JsonConvert.SerializeObject(model.Value);
            return Json(json);
        }
        public async Task<JsonResult> GetCities(int id)
        {
            var model = await _mediator.Send(new GetCitiesForChooseQuery(id));
            var json = JsonConvert.SerializeObject(model.Value);
            return Json(json);
        }

        [HttpPost]
        public async Task<JsonResult> Calculate(int sourceId, int destinationId, int weight)
        {
            PostPriceRequestModel req = new PostPriceRequestModel()
            {
                DestinationCityId = destinationId,
                SourceCityId = sourceId,
                Weight = weight
            };
            var calcuLate = await _mediator.Send(new PostCalculateCommand(req));
            var model = JsonConvert.SerializeObject(calcuLate.Value);
            return Json(model);
        }
    }
}
