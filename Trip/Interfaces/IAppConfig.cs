using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Models;

namespace Trip.Interfaces
{
    public interface IAppConfig
    {
        ConfigJSONModel Config { get; }
        string FilePath { get; }
    }
}
