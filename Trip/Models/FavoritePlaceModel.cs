using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Models
{
    public class FavoritePlaceModel
    {
        public string PlaceName { get; set; } = string.Empty;
        public string NickName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
