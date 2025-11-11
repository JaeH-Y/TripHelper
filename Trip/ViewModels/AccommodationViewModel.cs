using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trip.Interfaces.Services;
using Trip.Models;
using Trip.Messages;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Trip.Views;
using System.Windows;

namespace Trip.ViewModels
{
    public class AccommodationViewModel : ViewModelBase
    {
        public IFavoritePlaceService _favorites;
        public IAccommodationService _accommodations;
        private readonly IMessenger _messenger;
        public ObservableCollection<AccommodationModel> Accommodations { get; } = new ObservableCollection<AccommodationModel>();
        private string _mainText = "Accommodation ViewModel Binding Success!!";
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if(SetProperty(ref _isEditMode, value))
                {
                    IsReadOnlyMode = !value;
                    IsEditText = value ? "편집완료" : "편집하기";
                }
            }
        }
        private bool _isReadOnlyMode = true;
        public bool IsReadOnlyMode
        {
            get => _isReadOnlyMode;
            set => SetProperty(ref _isReadOnlyMode, value);
        }
        private bool _isAddMode;
        public bool IsAddMode
        {
            get => _isAddMode;
            set => SetProperty(ref _isAddMode, value);
        }
        private bool _isEditAble;
        public bool IsEditAble
        {
            get => _isEditAble;
            set => SetProperty(ref _isEditAble, value);
        }
        private bool _isAddAble;
        public bool IsAddAble
        {
            get => _isAddAble;
            set => SetProperty(ref _isAddAble, value);
        }
        private string _isEditText;
        public string IsEditText
        {
            get => _isEditText;
            set => SetProperty(ref _isEditText, value);
        }
        private Visibility _noneItemVisibility;
        public Visibility NoneItemVisibility
        {
            get => _noneItemVisibility;
            set => SetProperty(ref _noneItemVisibility, value);
        }
        private Visibility _hasItemVisibility;
        public Visibility HasItemVisibility
        {
            get => _hasItemVisibility;
            set => SetProperty(ref _hasItemVisibility, value);
        }

        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public AccommodationViewModel(IFavoritePlaceService favorites, IAccommodationService accomodations, IMessenger messenger)
        {
            _favorites = favorites;
            _accommodations = accomodations;
            _messenger = messenger;

            InitCommand();
            Initial();

            _messenger.Register<ChangedMessage, string>(this, MessageTokens.Accommodation,
                (_, msg) =>
                {

                });
        }
        private void InitCommand()
        {
            AddCommand = new RelayCommand(AddAccommodation);
            EditCommand = new RelayCommand(EditAccommodation);
        }
        private void Initial()
        {
            IsEditText = "편집하기";
            IsAddAble = true;
            IsEditAble = true;
            IsEditMode = false;
            ReadAccommodations();
        }
        private void ReadAccommodations()
        {
            NoneItemVisibility = Visibility.Collapsed;
            HasItemVisibility = Visibility.Collapsed;
            Accommodations.Clear();
            var list = _accommodations.LoadFavorite();

            if(list != null && list.Count > 0)
            {
                foreach(var item in list)
                {
                    Accommodations.Add(item);
                }
                HasItemVisibility = Visibility.Visible;
            }
            else
            {
                NoneItemVisibility = Visibility.Visible;
            }
        }
        private void AddAccommodation()
        {
            IsEditMode = false;
            IsEditAble = false;
            string titleT = "새로운 숙소 추가";
            string headT = "새로운 장소를 등록 해봐요!";
            string text1T = "NickName";
            string text2T = "PlaceName";
            string text3T = "Category";
            string okbtnT = "등록하기";
            string cancelT = "취소";

            var dialog = new FavoriteDialog(titleT, headT, text1T, text2T, text3T, okbtnT, cancelT);
            dialog.Owner = Application.Current.MainWindow;

            bool? dialogResult = dialog.ShowDialog();

            if(dialogResult == true)
            {
                switch (dialog.Result)
                {
                    case FavoriteDialog.FavoriteDialogResult.Ok:

                        var addData = new AccommodationModel
                        {
                            NickName = dialog.InputTexts[0],
                            PlaceName = dialog.InputTexts[1],
                            Category = dialog.InputTexts[2],
                            IsFavorite = true,
                        };
                        addData.FavoriteText = addData.IsFavorite ? "★" : "☆";
                        _accommodations.AddAccommodation(addData);
                        break;
                    case FavoriteDialog.FavoriteDialogResult.Cancel:

                        break;
                }
            }

            ReadAccommodations();
            IsEditAble = true;
        }
        private void EditAccommodation()
        {
            IsEditMode = !IsEditMode;
            IsAddAble = false;
            var accommodations = Accommodations;
            if (IsEditMode)
            {

            }
            else
            {
                var dialog = new FavoriteDialog("편집 완료 확인", "편집을 완료하시겠습니까?", "", "", "", "확인", "취소");

                dialog.Owner = Application.Current.MainWindow;
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    if (dialog.Result == FavoriteDialog.FavoriteDialogResult.Ok)
                    {
                        var list = new List<AccommodationModel>();
                        foreach (var item in accommodations)
                        {
                            list.Add(new AccommodationModel
                            {
                                PlaceName = item.PlaceName,
                                NickName = item.NickName,
                                Category = item.Category,
                                IsFavorite = item.IsFavorite,
                            });
                        }
                        _accommodations.EditJSON(list);
                        ReadAccommodations();
                        IsAddAble = true;
                    }
                    else IsEditMode = true;
                }
            }
        }
    }
}
