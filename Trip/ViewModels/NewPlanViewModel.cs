using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Trip.Interfaces.Services;
using Trip.Interfaces.ViewModels;
using Trip.Messages;
using Trip.Models;
using Trip.Views;
using static Trip.Services.PlanManagementService;

namespace Trip.ViewModels
{
    public class NewPlanViewModel : ViewModelBase, INewPlanViewModel
    {
        public ObservableCollection<AddPlaceOrPlanViewModel> Places { get; } = new();

        private string _mainText = "New Plan ViewModel Binding Success!!";
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }
        private string _randomRecommandedSpot = "방문하고 싶은 곳을 추가하시면 최적의 동선을 찾아드려요!";
        public string RandomRecommandedSpot
        {
            get => _randomRecommandedSpot;
            set => SetProperty(ref _randomRecommandedSpot, value);
        }
        private string _editPlaceBtnText = "편집하기";
        public string EditPlaceBtnText
        {
            get => _editPlaceBtnText;
            set => SetProperty(ref _editPlaceBtnText, value);
        }
        private bool _saveBtnEnable;
        public bool SaveBtnEnable
        {
            get => _saveBtnEnable;
            set => SetProperty(ref _saveBtnEnable, value);
        }
        private bool _directionBtnEnable;
        public bool DirectionBtnEnable
        {
            get => _directionBtnEnable;
            set => SetProperty(ref _directionBtnEnable, value);
        }

        private bool _isSaving = false;
        private bool _isAnalysis = false;
        public string PlanSaveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trips", "MyPlans");
        public string FavoritePlaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trips", "FavoritePlaces");

        public ICommand AddPlaceCommand { get; set; }
        public ICommand EditPlacesCommand { get; set; }
        public ICommand SavePlanCommand { get; set; }
        public ICommand GetDirectionsCommand { get; set; }
        
        private IFavoritePlaceService _favoritePlaceService;
        private IPlanManagementService _planService;
        private IShowDialogService _dialogS;
        private IGoogleAPIService _googleS;
        private IMessenger _messenger;
        public NewPlanViewModel(IFavoritePlaceService favoritePlaceService, IPlanManagementService planservice, IShowDialogService dialogs, IGoogleAPIService googles,
                                IMessenger messenger)
        {
            _favoritePlaceService = favoritePlaceService;
            _dialogS = dialogs;
            _planService = planservice;
            _googleS = googles;
            _messenger = messenger;
            
            _planService.FilePath = Path.Combine(PlanSaveFolder, "plans.json");

            AddPlaceCommand = new RelayCommand(AddPlace);
            EditPlacesCommand = new RelayCommand(EditPlaces);
            SavePlanCommand = new AsyncRelayCommand(SavePlaces, () => !_isSaving);
            GetDirectionsCommand = new AsyncRelayCommand(StartGetDirections, () => !_isAnalysis);

            SaveBtnEnable = true;

            PlanInit();
        }
        private void PlanInit()
        {
            Places.Clear();
            Places.Add(new AddPlaceOrPlanViewModel(this, _googleS)
            {
                PlaceName = "",
                NickName = "My Place Name",
                StayHour = 0,
                StayMinute = 0,
                AddFavoritePlace = false
            });
            Places.Add(new AddPlaceOrPlanViewModel(this, _googleS)
            {
                PlaceName = "",
                NickName = "My Place Name",
                StayHour = 0,
                StayMinute = 0,
                AddFavoritePlace = false
            });
        }
        private void AddPlace()
        {
            if(EditPlaceBtnText.Equals("완료")) return;
            int allindex = Places.Count > 0 ? Places.Count : 0;
            int insertindex = allindex;
            if (Places.Count > 1)
            {
                insertindex --;
            }
            Places.Insert(insertindex, new AddPlaceOrPlanViewModel(this, _googleS)
            {
                PlaceName = "",
                NickName = "My Place Name!!",
                StayHour = 0,
                StayMinute = 0,
                AddFavoritePlace = false
            });
        }
        private void EditPlaces()
        {
            var visible = Visibility.Visible;
            string nextText = "오류?";
            switch (EditPlaceBtnText)
            {
                case "편집하기":
                    visible = Visibility.Visible;
                    nextText = "완료";
                    break;
                case "완료":
                    visible = Visibility.Collapsed;
                    nextText = "편집하기";
                    break;
            }
            SaveBtnEnable = visible == Visibility.Visible ? false : true;
            if (Places.Count > 0)
            {
                foreach (var place in Places)
                {
                    place.EditVisibility = visible;
                }
            }
            EditPlaceBtnText = nextText;
        }
        private async Task SavePlaces()
        {
            if(_isSaving) return;

            try
            {
                _isSaving = true;
                SaveBtnEnable = false;

                string planName = "";
                var dialog = new FavoriteDialog("여행 계획 저장", "즐거운 여행 일정이 되길 바랍니다!", "계획 이름 :", "", "", "확인", "취소");

                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    switch (dialog.Result)
                    {
                        case FavoriteDialog.FavoriteDialogResult.Ok:
                            planName = dialog.TextBox1.Text;
                            break;
                        case FavoriteDialog.FavoriteDialogResult.Cancel:

                            return;
                        default:

                            return;
                    }
                }
                else if (result == null || result == false) return;
                if (string.IsNullOrEmpty(planName))
                {
                    _dialogS.NotReturnMessageBoxShow("여행 이름 오류", "계획 이름을 입력해주세요!");
                    return;
                }

                bool overwrite = false;
                var snapshot = await _planService.LoadJSONAsync();
                var exists = snapshot.Plans.ContainsKey(planName);
                if (exists)
                {
                    var res = _dialogS.ReturnMessageBoxShow(
                    "여행 계획 중복 확인",
                    "이미 해당 계획이 있습니다. 덮어쓰기 하시겠습니까?");
                    if (res != MessageBoxResult.Yes)
                    {
                        _dialogS.NotReturnMessageBoxShow("취소됨", "저장을 취소했습니다.");
                        return;
                    }
                    overwrite = true;
                }

                // 폴더 확인 후 없으면 생성
                Directory.CreateDirectory(PlanSaveFolder);

                // 뷰모델의 데이터를 모델에 옮겨 담아서 List화 후에 저장
                var dataList = Places.Select(vm => new PlaceModel
                {
                    NickName = vm.NickName,
                    PlaceName = vm.PlaceName,
                    StayHour = vm.StayHour,
                    StayMinute = vm.StayMinute,
                }).ToList();

                var saveResult = await _planService.SavePlanAsync(planName, dataList, overwrite);

                switch (saveResult)
                {
                    case SaveResult.Created:
                        _dialogS.NotReturnMessageBoxShow("저장 완료", "새 여행 계획을 저장했습니다!");
                        break;
                    case SaveResult.Overwritten:
                        _dialogS.NotReturnMessageBoxShow("덮어쓰기 완료", "기존 계획을 업데이트했습니다!");
                        break;
                    case SaveResult.ExistsButNotOverwritten:
                        // 이 경로는 정상적으로는 오지 않지만(우리는 overwrite=true), 동시성 안전망
                        _dialogS.NotReturnMessageBoxShow("저장 보류", "다른 저장이 선행되어 보류되었습니다.");
                        break;
                }
            }
            finally
            {
                _isSaving = false;
                SaveBtnEnable = true;
            }

        }
        public void EditMoveUp(AddPlaceOrPlanViewModel item)
        {
            if (Places.Count > 1)
            {
                int indexnum = Places.IndexOf(item);
                if (indexnum > 0)
                {
                    Places.Move(indexnum, indexnum - 1);
                }
            }
        }
        public void EditMoveDown(AddPlaceOrPlanViewModel item)
        {
            if (Places.Count > 1)
            {
                int indexnum = Places.IndexOf(item);
                if (indexnum < Places.Count -1)
                {
                    Places.Move(indexnum, indexnum + 1);
                }
            }
        }
        public void EditRemove(AddPlaceOrPlanViewModel item)
        {
            if (Places.Count > 1)
            {
                int indexnum = Places.IndexOf(item);
                Places.RemoveAt(indexnum);
            }
        }
        // 즐겨찾기 추가
        public void AddFavorite(AddPlaceOrPlanViewModel item)
        {
            item.AddFavoritePlace = !item.AddFavoritePlace;
            if (item.AddFavoritePlace)
            {
                SaveFavoritePlace(item);
            }
            else
            {
                RemoveFavoritePlace(item);
            }
        }
        public void SaveFavoritePlace(AddPlaceOrPlanViewModel item)
        {
            var modelData = new FavoritePlaceModel
            {
                PlaceName = item.PlaceName,
                NickName = item.NickName,
                IsFavorite = item.AddFavoritePlace,
                Category = "Default"
            };
            _favoritePlaceService.AddFavorite(modelData);
        }
        public void RemoveFavoritePlace(AddPlaceOrPlanViewModel item)
        {
            var modelData = new FavoritePlaceModel
            {
                PlaceName = item.PlaceName,
                NickName = item.NickName
            };
            _favoritePlaceService.DeleteFavorite(modelData);
        }
        // 지도 업데이트
        public void SendUri(Uri? uri, int? zoom, string? placeId)
        {
            _messenger.Send(new MapNavigateMessage(uri, zoom, placeId), MessageTokens.MapReload);
        }
        public void SendHtml(string? html)
        {
            _messenger.Send(new MapHtmlMessage(html), MessageTokens.MapReload);
        }

        //경로 분석
        private async Task StartGetDirections()
        {

        }
    }
}
