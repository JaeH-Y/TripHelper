using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Models;

namespace Trip.Interfaces.Services
{
    public interface IGoogleAPIService
    {
        Task<IReadOnlyList<PlaceSuggestion>> GoogleAutomaticCompleteAsync(string input, string language, string sessionToken, double? lat = null, double? lon = null, int? radiusMeters = null, CancellationToken token = default);

        Task<PlaceModel?> GetDetailsAsync(string placeId, string sessionToken, string language = "ko", CancellationToken token = default);

        Uri BuildEmbedUri(double lat, double lng, int zoom = 15, string mapType = "roadmap");

        string BuildEmbedHtml(string placeid, int zoom = 15, string mapType = "roadmap", string? language = "ko", string? region = "KR");
    }
}
