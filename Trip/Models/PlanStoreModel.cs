using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Models
{
    public class PlanStoreModel
    {
        public Dictionary<string, List<PlaceModel>> Plans { get; set; } = new();
        public int SchemaVersion { get; set; } = 1;
    }
}
