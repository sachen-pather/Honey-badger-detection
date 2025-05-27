using Honey_badger_detection.Models;

namespace Honey_badger_detection.Services
{
    public interface ICustomVisionService
    {
        Task<CustomVisionResponse> AnalyzeImageAsync(IFormFile image);
    }
}