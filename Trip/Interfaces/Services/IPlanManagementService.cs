using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Models;
using static Trip.Services.PlanManagementService;

namespace Trip.Interfaces.Services
{
    public interface IPlanManagementService
    {
        string FilePath { get; set; }

        Task<PlanStoreModel> LoadJSONAsync(CancellationToken token = default);
        Task SaveAllAsync(PlanStoreModel store, CancellationToken token = default);
        Task<SaveResult> SavePlanAsync(string planName, IEnumerable<PlaceModel> places, bool overwrite, CancellationToken token = default);
        Task<IReadOnlyList<PlaceModel>> LoadPlanAsync(string planName, CancellationToken ct = default);
        Task<IReadOnlyList<string>> ListPlanNamesAsync(CancellationToken ct = default);

    }
}
