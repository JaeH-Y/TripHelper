using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.ViewModels;

namespace Trip.Interfaces.ViewModels
{
    public interface IPlanCabinetViewModel
    {
        void RemovePlan(PlanDictonaryViewModel item);
    }
}
