using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trip.Interfaces.Services;
using Trip.Models;
using Trip.Views;
using Trip.Messages;

namespace Trip.ViewModels
{
    public class LikePlaceViewModel : ViewModelBase //편집 후 다시 불러오기 필요(인터페이스 필요)
    {
        // 즐겨찾기 표시용 Observable
        public ObservableCollection<FavoritePlaceManagerViewModel> FavoritePlaces { get; set; } = new();
        
        // JSON 파싱용 변수
        public readonly string FavoritePlaceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Trips", "FavoritePlaces");
        public string FilePath ;

        // 바인딩 변수
        private string _mainText = "LikePlace ViewModel Binding Success!!";
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }
        private string _isEditText;
        public string IsEditText
        {
            get => _isEditText;
            set => SetProperty(ref _isEditText, value);
        }
        private bool _isEditMode = false;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    IsEditText = value ? "편집완료" : "편집하기";
                }
            }
        }
        private bool _isEditAble = false;
        public bool IsEditAble
        {
            get => _isEditAble;
            set => SetProperty(ref _isEditAble, value);
        }
        private bool _isAddAble = false;
        public bool IsAddAble
        {
            get => _isAddAble;
            set => SetProperty(ref _isAddAble, value);
        }
        private Visibility _hasFavoriteVisibility = Visibility.Visible;
        public Visibility HasFavoriteVisibility
        {
            get => _hasFavoriteVisibility;
            set
            {
                if(SetProperty(ref _hasFavoriteVisibility, value))
                {
                    EmptyFavoriteVisibility = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }
        private Visibility _emptyFavoriteVisibility;
        public Visibility EmptyFavoriteVisibility
        {
            get => _emptyFavoriteVisibility;
            set => SetProperty(ref _emptyFavoriteVisibility, value);
        }
        
        // 커맨드
        public ICommand AddFavoriteCommand { get; set; }
        public ICommand EditFavoriteCommand { get; set; }
        // 구독
        public IFavoritePlaceService _service;
        private readonly IMessenger _messenger;
        public LikePlaceViewModel(IFavoritePlaceService service, IMessenger messenger)
        {
            AddFavoriteCommand = new RelayCommand(ManualAddFavorite);
            EditFavoriteCommand = new RelayCommand(EditFavorites);
            _service = service;
            _messenger = messenger;
            Initialize();
            _messenger.Register<ChangedMessage, string>(this,
            MessageTokens.FavoritePlaces,
            (_, msg) =>
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    Application.Current.Dispatcher.Invoke(() => ReadFavoritePlaceJSON(FilePath));
                else
                    ReadFavoritePlaceJSON(FilePath);
            });
        }
        private void Initialize()
        {
            IsAddAble = true;
            IsEditAble = true;
            IsEditMode = false;
            IsEditText = "편집하기";
            FilePath = Path.Combine(FavoritePlaceFolder, "FavoritePlaces.json");
            ReadFavoritePlaceJSON(FilePath);
        }
        private void ReadFavoritePlaceJSON(string jsonpath)
        {
            FavoritePlaces.Clear();
            HasFavoriteVisibility = Visibility.Collapsed;

            var list = _service.LoadFavorite() ?? new List<FavoritePlaceModel>();
            foreach (var item in list)
            {
                FavoritePlaces.Add(new FavoritePlaceManagerViewModel
                {
                    PlaceName = item.PlaceName,
                    NickName = item.NickName,
                    Category = item.Category,
                    IsFavorite = item.IsFavorite,
                    IsEditMode = false,
                    IsEditModeVisibility = Visibility.Collapsed
                });
            }
            
            HasFavoriteVisibility = FavoritePlaces.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void ManualAddFavorite()
        {
            IsEditAble = false;

            // ShowDialog로 띄워야겠다.
            string titleT = "새로운 즐겨찾기 추가!";
            string headT = "새로운 장소를 등록 해봐요!";
            string text1T = "NickName";
            string text2T = "PlaceName";
            string text3T = "Category";
            string okbtnT = "등록하기";
            string cancelT = "취소";
            var dialog = new FavoriteDialog(titleT, headT, text1T, text2T, text3T, okbtnT, cancelT);

            dialog.Owner = Application.Current.MainWindow;

            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult == true)
            {
                switch (dialog.Result)
                {
                    case FavoriteDialog.FavoriteDialogResult.Ok:

                        var addData = new FavoritePlaceModel
                        {
                            NickName = dialog.InputTexts[0],
                            PlaceName = dialog.InputTexts[1],
                            Category = dialog.InputTexts[2],
                            IsFavorite = true,
                        };
                        _service.AddFavorite(addData);
                        break;
                    case FavoriteDialog.FavoriteDialogResult.Cancel:

                        break;
                }
            }
            IsEditAble = true;
            ReadFavoritePlaceJSON(FilePath);
        }
        private void EditFavorites()
        {
            IsEditMode = !IsEditMode;
            IsAddAble = false;
            var favorites = FavoritePlaces;
            if (IsEditMode)
            {
                foreach (var item in favorites)
                {
                    item.IsEditMode = true;
                }
            }
            else
            {
                //#if DEBUG
                var dialog = new FavoriteDialog("편집 완료 확인", "편집을 완료하시겠습니까?", "", "", "", "확인", "취소");
                
                dialog.Owner = Application.Current.MainWindow;
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    if (dialog.Result == FavoriteDialog.FavoriteDialogResult.Ok)
                    {
                        var list = new List<FavoritePlaceModel>();
                        foreach (var item in favorites)
                        {
                            item.IsEditMode = false;
                            list.Add(new FavoritePlaceModel
                            {
                                PlaceName = item.PlaceName,
                                NickName = item.NickName,
                                Category = item.Category,
                                IsFavorite = item.IsFavorite,
                            });
                        }
                        _service.EditFavorite(list);
                        ReadFavoritePlaceJSON(FilePath);
                        IsAddAble = true;
                    }
                    else IsEditMode = true;
                }
            }
        }
    }
}
