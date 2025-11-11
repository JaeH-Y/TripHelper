using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Models
{
    public class AccommodationModel
    {
        public string PlaceName { get; set; }
        public string NickName { get; set; }
        public string Category { get; set; }
        public string FavoriteText { get; set; }
        public bool IsFavorite { get; set; }
    }
}
