using CommunityToolkit.Mvvm.Messaging;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Trip.Interfaces.Services;
using Trip.Messages;
using Trip.Models;

namespace Trip.Services
{
    public class PlanManagementService : IPlanManagementService
    {
        // 저장 시 중복 확인
        public enum SaveResult { Created, Overwritten, ExistsButNotOverwritten }

        private readonly SemaphoreSlim _task = new (1, 1);
        private readonly SemaphoreSlim _saveTask = new (1, 1);
        private readonly JsonSerializerOptions _json = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public string FilePath { get; set; }

        private IShowDialogService _dialogS;
        private IMessenger _messenger;
        public PlanManagementService(IShowDialogService dialogs, IMessenger messenger)
        {
            _dialogS = dialogs;
            _messenger = messenger;
        }
        public async Task<PlanStoreModel> LoadJSONAsync(CancellationToken token = default)
        {
            await _task.WaitAsync(token);

            try
            {
                if (!File.Exists(FilePath))
                {
                    return new PlanStoreModel();
                }

                using var fs = File.OpenRead(FilePath);
                var store = await JsonSerializer.DeserializeAsync<PlanStoreModel>(fs, _json, token);

                return store ?? new PlanStoreModel();
            }
            finally
            {
                _task.Release();
            }
        }
        public async Task SaveAllAsync(PlanStoreModel store, CancellationToken token = default)
        {
            await _task.WaitAsync(token);
            try
            {
                using var fs = File.Create(FilePath);
                await JsonSerializer.SerializeAsync(fs, store, _json, token);
            }
            finally { _task.Release(); }
        }
        public async Task<SaveResult> SavePlanAsync(string planName, IEnumerable<PlaceModel> places, bool overwrite, CancellationToken token = default)
        {
            await _saveTask.WaitAsync(token);
            try
            {
                // 기존 저장 불러오기
                var store = await LoadJSONAsync(token);

                // 중복 확인
                var exists = store.Plans.ContainsKey(planName);
                if (exists && !overwrite)
                    return SaveResult.ExistsButNotOverwritten;

                // 딕셔너리에 저장
                store.Plans[planName] = places?.ToList() ?? new List<PlaceModel>();

                await SaveAllAsync(store, token);
                _messenger.Send(new ChangedMessage(ChangeKind.Reload), MessageTokens.PlanCabinet);
                return exists ? SaveResult.Overwritten : SaveResult.Created;
            }
            finally { _saveTask.Release(); }
        }
        public async Task<IReadOnlyList<PlaceModel>> LoadPlanAsync(string planName, CancellationToken ct = default)
        {
            var store = await LoadJSONAsync(ct);
            return store.Plans.TryGetValue(planName, out var list)
                ? list
                : Array.Empty<PlaceModel>();
        }

        public async Task<IReadOnlyList<string>> ListPlanNamesAsync(CancellationToken ct = default)
        {
            var store = await LoadJSONAsync(ct);
            return store.Plans.Keys.OrderBy(x => x).ToList();
        }
    }
}
