using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Trip.ViewModels
{
    public class FavoritePlaceManagerViewModel : ViewModelBase
    {
        private string _placeName;
        public string PlaceName
        {
            get => _placeName;
            set => SetProperty(ref _placeName, value);
        }
        private string _nickName;
        public string NickName
        {
            get => _nickName;
            set => SetProperty(ref _nickName, value);
        }
        private string _category;
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }
        private string _favoriteText = "☆";
        public string FavoriteText
        {
            get => _favoriteText;
            set => SetProperty(ref _favoriteText, value);
        }
        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (SetProperty(ref _isFavorite, value))
                {
                    FavoriteText = value ? "★" : "☆";
                }
            }
        }
        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if(SetProperty(ref _isEditMode, value))
                {
                    IsEditModeVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
        private Visibility _isEditModeVisibility;
        public Visibility IsEditModeVisibility
        {
            get => _isEditModeVisibility;
            set
            {
                if(SetProperty(ref _isEditModeVisibility, value))
                {
                    IsNotEditModeVisibility = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }
        private Visibility _isNotEditModeVisibility;
        public Visibility IsNotEditModeVisibility
        {
            get => _isNotEditModeVisibility;
            set => SetProperty(ref _isNotEditModeVisibility, value);
        }
        public ICommand FavoriteChangeCommand { get; set; }

        public FavoritePlaceManagerViewModel()
        {
            FavoriteChangeCommand = new RelayCommand(FavoriteChange);
        }

        private void FavoriteChange()
        {
            IsFavorite = !IsFavorite;
        }
    }

}
