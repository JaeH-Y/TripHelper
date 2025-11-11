using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trip.Interfaces.Services;
using Trip.Interfaces.ViewModels;
using Trip.Messages;
using Trip.Models;

namespace Trip.ViewModels
{
    public class PlanCabinetViewModel : ViewModelBase, IPlanCabinetViewModel
    {
        public ObservableCollection<PlanDictonaryViewModel> Plans { get; set; } = new();
        private readonly SemaphoreSlim _loadGate = new(1, 1);
        private IPlanManagementService _planS;
        private IMessenger _messenger;
        private IShowDialogService _dialogS;

        private string _mainText = "PlanCabinet ViewModel Binding Success!!";
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }

        private string _editText = "편집하기";
        public string EditText
        {
            get => _editText;
            set => SetProperty(ref _editText, value);
        }

        public ICommand AddPlanCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public PlanCabinetViewModel(IPlanManagementService planS, IMessenger messenger, IShowDialogService dialogService)
        {
            _planS = planS;
            _messenger = messenger;
            _dialogS = dialogService;

            Initial();
            InitialCommand();
           _ = LoadPlansAsync();
        }

        private void InitialCommand()
        {
            AddPlanCommand = new RelayCommand(AddPlan);
            EditCommand = new AsyncRelayCommand(EditScadule);
        }
        private void Initial()
        {
            _messenger.Register<ChangedMessage, string>(this,
                MessageTokens.PlanCabinet,
                (_, msg) =>
                {
                    if (!Application.Current.Dispatcher.CheckAccess())
                        Application.Current.Dispatcher.Invoke(() => LoadPlansAsync());
                    else _ = LoadPlansAsync();
                });
        }
        private void AddPlan()
        {
            if (EditText.Equals("완료"))
            {
                _dialogS.NotReturnMessageBoxShow("편집 중 에러", "편집 중입니다!");
                return;
            }
            _messenger.Send(new ChangedMessage(ChangeKind.Reload), MessageTokens.NewPlanPageOpen);
        }
        private async Task EditScadule()
        {

            var visible = Visibility.Visible;
            string nextText = string.Empty;
            switch (EditText)
            {
                case "편집하기":
                    nextText = "완료";
                    break;
                case "완료":
                    nextText = "편집하기";
                    visible = Visibility.Collapsed;
                    await SaveEditedPlan();
                    _dialogS.NotReturnMessageBoxShow("편집 완료", "계획 보관함 편집이 완료되었습니다!");
                    break;
            }

            foreach (var item in Plans)
            {
                item.EditVisibility = visible;
            }
            EditText = nextText;
        }
        public void RemovePlan(PlanDictonaryViewModel item)
        {
            if(Plans.Count > 0)
            {
                int indexNum = Plans.IndexOf(item);
                Plans.RemoveAt(indexNum);
            }
        }
        private async Task LoadPlansAsync()
        {
            try
            {
                await _loadGate.WaitAsync();

                Plans.Clear();
                var store = await _planS.LoadJSONAsync().ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Plans.Clear();
                    foreach (var kv in store.Plans.OrderBy(x => x.Key))
                        Plans.Add(new PlanDictonaryViewModel(kv.Key, kv.Value, this));
                }, System.Windows.Threading.DispatcherPriority.Background);

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Load Plans Exception : {ex}");
            }
            finally
            {
                _loadGate.Release();
            }
        }
        private async Task SaveEditedPlan()
        {
            try
            {
                await _loadGate.WaitAsync();

                var currentNames = await Application.Current.Dispatcher.InvokeAsync(() =>
                                        Plans.Where(p => !string.IsNullOrWhiteSpace(p.PlanName))
                                             .Select(p => p.PlanName)
                                             .ToHashSet(StringComparer.Ordinal)  // 필요 시 OrdinalIgnoreCase
                                    );

                var store = await _planS.LoadJSONAsync().ConfigureAwait(false);

                if (store?.Plans == null)
                    store = new PlanStoreModel(); // 방어적

                if (currentNames.Count > 0)
                {
                    // Plans에 포함되지 않는 항목 List화
                    var keysToRemove = store.Plans.Keys
                                  .Where(k => !currentNames.Contains(k))
                                  .ToList();

                    // 리스트에 담긴 항목 제거
                    foreach (var k in keysToRemove)
                        store.Plans.Remove(k);
                }
                else
                {
                    store.Plans.Clear();
                }
                await _planS.SaveAllAsync(store);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _loadGate.Release();
            }
        }
    }
}
