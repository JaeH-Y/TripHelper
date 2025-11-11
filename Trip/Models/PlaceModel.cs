using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Models
{
    public record PlaceSuggestion(string Description, string PlaceId);
    public class PlaceModel
    {
        public string PlaceName { get; set; }
        public string NickName { get; set; }
        public double StayHour { get; set; }
        public double StayMinute { get; set; }
        public string PlaceId { get; init; }
        public string Name { get; init; }
        public string FormattedAddress { get; init; }
        public double Lat { get; init; }
        public double Lng { get; init; }
        public string PhoneNumber { get; init; }
        public string Url { get; init; }
    }
}
