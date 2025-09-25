using ErrorOr;
 
using Gtm.Contract.SiteContract.SliderContract.Query;
using MediatR;
using Utility.Appliation.FileService;

namespace Gtm.Application.SiteServiceApp.SliderApp.Query
{
    public record GetAllForUIQuery : IRequest<ErrorOr<List<SliderForUi>>>;

    public class GetAllForUIQueryHandler : IRequestHandler<GetAllForUIQuery, ErrorOr<List<SliderForUi>>>
    {
        private readonly ISliderRepository _sliderRepository;

        public GetAllForUIQueryHandler(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository ?? throw new ArgumentNullException(nameof(sliderRepository));
        }

        public async Task<ErrorOr<List<SliderForUi>>> Handle(GetAllForUIQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = await _sliderRepository.GetAllByQueryAsync(s => s.Active);
                var activeSliders = query.ToList(); // Materialize the query

                if (!activeSliders.Any())
                {
                    return new List<SliderForUi>();
                }

                return activeSliders
                    .Select(s => new SliderForUi
                    {
                        ImageAlt = s.ImageAlt,
                        ImageName = Path.Combine(FileDirectories.SliderImageDirectory, s.ImageName),
                        Url = s.Url
                    })
                    .ToList();
            }
            catch (OperationCanceledException)
            {
                return Error.Conflict("Slider.OperationCancelled", "عملیات دریافت اسلایدرها لغو شد");
            }
            catch (Exception ex)
            {
                return Error.Failure("Slider.FetchError", $"خطا در دریافت اسلایدرها: {ex.Message}");
            }
        }
    }
}
