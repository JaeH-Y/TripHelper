using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using Trip.Models;

namespace Trip.ViewModels
{
    public class AddPlaceOrPlanViewModel : ViewModelBase
    {
        // 콤보박스
        public ObservableCollection<PlaceSuggestion> Suggestions { get; } = new ObservableCollection<PlaceSuggestion>();
        // SelectedItem
        private PlaceSuggestion? _selectedSuggestion;
        public PlaceSuggestion? SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                if(SetProperty(ref _selectedSuggestion, value) && value != null)
                {
                    IsPopupOpen = false;
                    IsAddressSelected = true;
                    PlaceName = value.Description;
                    _ = GetPlaceDetail(value.PlaceId);
                }
            }
        }
        // 상세 표시용(지도 등)
        private PlaceModel? _selectedPlace;
        public PlaceModel? SelectedPlace
        {
            get => _selectedPlace;
            set
            {
                if(SetProperty(ref _selectedPlace, value))
                {
                    UpdateMapUri();
                }
            }
        }
        private Uri? _mapUri;
        public Uri? MapUri
        {
            get => _mapUri;
            set => SetProperty(ref _mapUri, value);
        }

        private string? _mapHtml;
        public string? MapHtml
        {
            get => _mapHtml;
            set => SetProperty(ref _mapHtml, value);
        }

        // 팝업 변경 딜레이
        private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(350);

        // 취소 토큰
        private CancellationTokenSource? _typingCts;
        private CancellationTokenSource? _requestCts;

        private string _sessionToken = Guid.NewGuid().ToString("N");

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => SetProperty(ref _isPopupOpen, value);
        }
        private bool _isAddressSelected = false;
        public bool IsAddressSelected
        {
            get => _isAddressSelected;
            set => SetProperty(ref _isAddressSelected, value);
        }
        private string _placeName = "";
        public string PlaceName
        {
            get => _placeName;
            set
            {
                if(SetProperty(ref _placeName, value))
                {
                    AddFavoritePlace = false;
                    if(value != null)
                    {
                        OnSearchTextChanged(value);
                        Console.WriteLine($"팝업 준비 시작");
                    }
                }
            }
        }
        private string _nickName = "My Place Name";
        public string NickName
        {
            get => _nickName;
            set
            {
                if (SetProperty(ref _nickName, value))
                {
                    AddFavoritePlace = false;
                }
            }
        }
        private double _stayHour = 0;
        public double StayHour
        {
            get => _stayHour;
            set => SetProperty(ref _stayHour, value);
        }
        private double _stayMinute = 0;
        public double StayMinute
        {
            get => _stayMinute;
            set => SetProperty(ref _stayMinute, value);
        }
        private bool _addFavoritePlace;
        public bool AddFavoritePlace
        {
            get => _addFavoritePlace;
            set
            {
                if(SetProperty(ref _addFavoritePlace, value))
                {
                    if (value)
                    {
                        FavoriteText = "★";
                    }
                    else
                    {
                        FavoriteText = "☆";
                    }
                }
            }
        }
        private string _favoriteText = "☆";
        public string FavoriteText
        {
            get => _favoriteText;
            set => SetProperty(ref _favoriteText, value);
        }
        private string _category = "Default";
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }
        private Visibility _editVisibility = Visibility.Collapsed;
        public Visibility EditVisibility
        {
            get => _editVisibility;
            set => SetProperty(ref _editVisibility, value);
        }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand AddFavoriteCommand { get; }
        public ICommand CallMapUri { get; }

        private readonly INewPlanViewModel _newPlanVM;
        private IGoogleAPIService _googleS;
        // 편집모드 생성자
        public AddPlaceOrPlanViewModel(INewPlanViewModel newvm, IGoogleAPIService googles)
        {
            _newPlanVM = newvm;
            _googleS = googles;
            MoveDownCommand = new RelayCommand(PlaceMoveDown);
            MoveUpCommand = new RelayCommand(PlaceMoveUp);
            RemoveCommand = new RelayCommand(RemovePlace);
            AddFavoriteCommand = new RelayCommand(AddFavorite);
            CallMapUri = new RelayCommand(SendUriMessage);
        }
        // 여행 계획 Json 저장, 기본 불러오기 생성자
        public AddPlaceOrPlanViewModel(PlaceModel model)
        {
            PlaceName = model.PlaceName;
            NickName = model.NickName;
            StayHour = model.StayHour;
            StayMinute = model.StayMinute;
            // 편집 모드가 아니니 EditVisibility 기본값(Collapsed) 유지
            // 커맨드도 주입하지 않음
        }
        public void PlaceMoveDown()
        {
            _newPlanVM.EditMoveDown(this);
        }
        public void PlaceMoveUp()
        {
            _newPlanVM.EditMoveUp(this);
        }
        public void RemovePlace()
        {
            _newPlanVM.EditRemove(this);
        }
        public void AddFavorite()
        {
            _newPlanVM.AddFavorite(this);
        }
        private void OnSearchTextChanged(string? value)
        {
            if (IsAddressSelected)
            {
                _typingCts?.Cancel();
                IsAddressSelected = false;
                return;
            }
            // 입력 변경 → 디바운스 후 자동완성 호출
            _typingCts?.Cancel();
            _typingCts = new CancellationTokenSource();
            _ = DebouncedAutocompleteAsync(value, _typingCts.Token);
        }
        private async Task DebouncedAutocompleteAsync(string? input, CancellationToken ct)
        {
            try
            {
                Console.WriteLine($"함수 호출됨");
                if (string.IsNullOrWhiteSpace(input))
                {
                    Suggestions.Clear();
                    IsPopupOpen = false;
                    Console.WriteLine($"input Is Null");
                    return;
                }

                await Task.Delay(_debounce, ct); // 디바운스
                Console.WriteLine($"{_debounce} 대기");

                IsBusy = true;
                ErrorMessage = null;

                // (옵션) 위치 바이어스: 서울시청 근방 예시
                double? lat = null, lng = null; int? radius = null;
                // lat = 37.5662952; lng = 126.9779451; radius = 30000;

                Console.WriteLine($"리스트 추출 시작");

                DateTime startT = DateTime.Now;
                var list = await _googleS.GoogleAutomaticCompleteAsync(input, _sessionToken, "ko", lat, lng, radius, ct);
                DateTime endT = DateTime.Now;

                Console.WriteLine($"소요시간 : {endT -  startT}");
                // UI 컬렉션 갱신
                Suggestions.Clear();
                foreach (var s in list)
                    Suggestions.Add(s);
                if(Suggestions.Count > 0) IsPopupOpen = true;
            }
            catch (OperationCanceledException) { /* 사용자가 타이핑 계속 → 정상취소 */ }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void UpdateMapUri()
        {
            if (SelectedPlace == null) { MapUri = null; return; }
            MapHtml = _googleS.BuildEmbedHtml(SelectedPlace.PlaceId);
        }
        private async Task GetPlaceDetail(string placeId)
        {
            SelectedPlace = await _googleS.GetDetailsAsync(placeId, _sessionToken);
        }
        private void SendUriMessage()
        {
            _newPlanVM.SendHtml(MapHtml);
        }
    }
}
