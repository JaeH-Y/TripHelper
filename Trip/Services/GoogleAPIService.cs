using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Trip.Interfaces.Services;
using Trip.Models;
using static System.Net.WebRequestMethods;

namespace Trip.Services
{
    public class GoogleAPIService : IGoogleAPIService
    {
        private readonly HttpClient httpClient = new HttpClient();
        public readonly string _apiKey;
        
        public GoogleAPIService(string apiKey, HttpClient http)
        {
            _apiKey = string.IsNullOrWhiteSpace(apiKey) ? throw new ArgumentNullException(nameof(apiKey)) : apiKey; ;
            httpClient = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<IReadOnlyList<PlaceSuggestion>> GoogleAutomaticCompleteAsync(string input, string sessionToken, string language = "ko", double? lat = null, double? lon = null, int? radiusMeters = null, CancellationToken token = default)
        {
            // 콤보박스 리스트 생성
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<PlaceSuggestion>();    // 빈 리스트 전달

            // 기본 url 생성
            var url = $"https://maps.googleapis.com/maps/api/place/autocomplete/json" +
                  $"?input={Uri.EscapeDataString(input)}" +
                  $"&language={language}" +
                  $"&key={_apiKey}" +
                  $"&sessiontoken={sessionToken}";

            // 좌표값, 반경 값 있을 경우 반영
            if (lat.HasValue && lon.HasValue)
            {
                url += $"&location={lat.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"{lon.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }
            if (radiusMeters.HasValue) url += $"&radius={radiusMeters.Value}";

            using var resp = await httpClient.GetAsync(url, token);
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync(token);
            using var doc = await JsonDocument.ParseAsync(s, cancellationToken: token);

            var status = doc.RootElement.GetProperty("status").GetString();
            if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "ZERO_RESULTS", StringComparison.OrdinalIgnoreCase))
            {
                var errMsg = doc.RootElement.TryGetProperty("error_message", out var e) ? e.GetString() : status;
                throw new InvalidOperationException($"Autocomplete API error: {errMsg}");
            }

            var list = new List<PlaceSuggestion>();
            if (doc.RootElement.TryGetProperty("predictions", out var preds))
            {
                foreach (var p in preds.EnumerateArray())
                {
                    var desc = p.TryGetProperty("description", out var d) ? d.GetString() : null;
                    var pid = p.TryGetProperty("place_id", out var id) ? id.GetString() : null;
                    if (!string.IsNullOrWhiteSpace(desc) && !string.IsNullOrWhiteSpace(pid))
                        list.Add(new PlaceSuggestion(desc!, pid!));
                }
            }
            return list;
        }

        public async Task<PlaceModel?> GetDetailsAsync(string placeId, string sessionToken, string language = "ko", CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(placeId)) return null;

            var url = $"https://maps.googleapis.com/maps/api/place/details/json" +
                  $"?place_id={Uri.EscapeDataString(placeId)}" +
                  $"&language={language}" +
                  $"&key={_apiKey}" +
            $"&sessiontoken={sessionToken}";

            using var resp = await httpClient.GetAsync(url, token);
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync(token);
            using var doc = await JsonDocument.ParseAsync(s, cancellationToken: token);

            var status = doc.RootElement.GetProperty("status").GetString();
            if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
            {
                var errMsg = doc.RootElement.TryGetProperty("error_message", out var e) ? e.GetString() : status;
                throw new InvalidOperationException($"Details API error: {errMsg}");
            }

            var result = doc.RootElement.GetProperty("result");
            var name = result.TryGetProperty("name", out var n) ? n.GetString() : null;
            var addr = result.TryGetProperty("formatted_address", out var a) ? a.GetString() : null;
            var urlProp = result.TryGetProperty("url", out var u) ? u.GetString() : null;
            var phone = result.TryGetProperty("formatted_phone_number", out var p) ? p.GetString() : null;

            var loc = result.GetProperty("geometry").GetProperty("location");
            var lat = loc.GetProperty("lat").GetDouble();
            var lng = loc.GetProperty("lng").GetDouble();

            return new PlaceModel
            {
                PlaceId = placeId,
                Name = name ?? "",
                FormattedAddress = addr ?? "",
                Lat = lat,
                Lng = lng,
                PhoneNumber = phone ?? "",
                Url = urlProp ?? ""
            };
        }
        public Uri BuildEmbedUri(double lat, double lng, int zoom = 15, string mapType = "roadmap")
        {
            // 줌/드래그만 되는 뷰 모드
            // https://www.google.com/maps/embed/v1/view?key=...&center=lat,lng&zoom=15&maptype=roadmap
            var url = $"https://www.google.com/maps/embed/v1/view?key={_apiKey}" +
                      $"&center={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $",{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&zoom={zoom}&maptype={mapType}";
            return new Uri(url);
        }

        public string BuildEmbedHtml(string placeid, int zoom = 15, string mapType = "roadmap", string? language = "ko", string? region = "KR")
        {
            var q = Uri.EscapeDataString($"place_id:{placeid}");

            var src = $"https://www.google.com/maps/embed/v1/place?key={_apiKey}&q={q}&zoom={zoom}" +
              (language is { Length: > 0 } ? $"&language={language}" : "") +
              (region is { Length: > 0 } ? $"&region={region}" : "");

            return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
                   <style>html,body{{height:100%;margin:0}}iframe{{width:100%;height:100%;border:0}}</style>
                   </head><body><iframe src='{src}' allowfullscreen loading='lazy'></iframe></body></html>";
        }
        // 위경도로 조회(핀X)
        /*public string BuildEmbedHtml(double lat, double lng, int zoom = 15, string mapType = "roadmap", string? language = "ko", string? region = "KR")
        {
            var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lngStr = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var src = $"https://www.google.com/maps/embed/v1/view?key={_apiKey}" +
                      $"&center={latStr},{lngStr}&zoom={zoom}&maptype={mapType}" +
                      (language is { Length: > 0 } ? $"&language={language}" : "") +
                      (region is { Length: > 0 } ? $"&region={region}" : "");

            return $@"<!DOCTYPE html>
                    <html><head><meta charset='utf-8'>
                    <style>html,body{{height:100%;margin:0}}iframe{{width:100%;height:100%;border:0}}</style>
                    </head><body>
                      <iframe src='{src}' allowfullscreen loading='lazy'></iframe>
                    </body></html>";
        }*/
    }
}
