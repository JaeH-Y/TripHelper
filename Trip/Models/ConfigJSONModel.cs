using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Models
{
    public class ConfigJSONModel
    {
        // Mode 0 = Debug, 1 = Release
        public int Mode { get; set; } = 0;

        public string LocalUrl { get; set; } = string.Empty;
        public string HttpUrl { get; set; } = string.Empty;

        // IsTimeRequest 0 = NoRequest, 1 = LoopRequest
        public int IsTimeRequest { get; set; } = 0;

    }
}
