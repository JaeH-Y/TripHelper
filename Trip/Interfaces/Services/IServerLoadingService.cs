using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.Interfaces.Services
{
    interface IServerLoadingService
    {
        Task<string> GetServerStatus();
    }
}
