using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Trip.Interfaces.ViewModels;
using Trip.Messages;

namespace Trip.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private IMessenger _messenger;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if(SetProperty(ref _currentViewModel, value))
                {
                    NotifyMessage = $"ViewModel 전환";
                }
            }
        }
        private string _headerText;
        public string HeaderText
        {
            get => _headerText;
            set => SetProperty(ref _headerText, value);
        }
        private string _notifyMessage;
        public string NotifyMessage
        {
            get => _notifyMessage;
            set
            {
                SetProperty(ref _notifyMessage, value);
            }
        }
        private bool _contentuseable;
        public bool ContentUseable
        {
            get => _contentuseable;
            set
            {
                if(SetProperty(ref _contentuseable, value))
                {
                    if (value)
                    {
                        IsFindVisibility = Visibility.Visible;
                    }
                    else
                    {
                        NullVisibility = Visibility.Visible;
                    }
                }
            }
        }
        private Visibility _nullVisibility;
        public Visibility NullVisibility
        {
            get => _nullVisibility;
            set
            {
                if(SetProperty(ref _nullVisibility, value))
                {
                    IsFindVisibility = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        private Visibility _isfindVisibility;
        public Visibility IsFindVisibility
        {
            get => _isfindVisibility;
            set
            {
                if (SetProperty(ref _isfindVisibility, value))
                {
                    NullVisibility = value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        // 지도 업데이트
        private Uri? _viewUri;
        public Uri? ViewUri
        {
            get => _viewUri;
            set
            {
                if(value == null) { NullVisibility = Visibility.Visible; }
                else
                {
                    IsFindVisibility = Visibility.Visible;
                }
                SetProperty(ref _viewUri, value);
            }
        }
        private string? _viewHtml;
        public string? ViewHtml
        {
            get => _viewHtml;
            set
            {
                if (value == null) { NullVisibility = Visibility.Visible; }
                else
                {
                    IsFindVisibility = Visibility.Visible;
                }
                SetProperty(ref _viewHtml, value);
            }
        }

        public MainViewModel(IServiceProvider serviceProvider, IMessenger messenger)
        {
            _serviceProvider = serviceProvider;
            _messenger = messenger;
            HeaderText = "방문을 환영합니다!";
            ViewModelChange("NewPlan");

            _messenger.Register<ChangedMessage, string>(this,
                MessageTokens.NewPlanPageOpen,
                (_, msg) =>
                {
                    if (!Application.Current.Dispatcher.CheckAccess())
                        Application.Current.Dispatcher.Invoke(() => ViewModelChange("NewPlan"));
                    else ViewModelChange("NewPlan");
                });

            _messenger.Register<MapNavigateMessage, string>(this, MessageTokens.MapReload, (_, msg) =>
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    Application.Current.Dispatcher.Invoke(() => UpdateUri(msg));
                else UpdateUri(msg);
            });

            _messenger.Register<MapHtmlMessage, string>(this, MessageTokens.MapReload, (_, msg) =>
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    Application.Current.Dispatcher.Invoke(() => UpdateHtml(msg));
                else UpdateHtml(msg);
            });
        }
        [RelayCommand]
        public void ViewModelChange(object? parameter)
        {
            if(parameter is string message && !string.IsNullOrEmpty(message))
            {
                switch (message)
                {
                    case "NewPlan":
                        CurrentViewModel = _serviceProvider.GetRequiredService<NewPlanViewModel>();
                        HeaderText = "새로운 계획을 세워보세요!";
                        break;
                    case "Accommodation":
                        CurrentViewModel = _serviceProvider.GetRequiredService<AccommodationViewModel>();
                        HeaderText = "내 숙소 관리";
                        break;
                    case "LikedPlace":
                        CurrentViewModel = _serviceProvider.GetRequiredService<LikePlaceViewModel>();
                        HeaderText = "☆내가 즐겨찾는 장소☆";
                        break;
                    case "PlanCabinet":
                        CurrentViewModel = _serviceProvider.GetRequiredService<PlanCabinetViewModel>();
                        break;
                    case "Setting":
                        CurrentViewModel = _serviceProvider.GetRequiredService<SettingViewModel>();
                        break;
                }
            }
            else
            {
                NotifyMessage = $"전환하고자 하는 ViewModel 없음";
            }
        }

        public void ShowMessageBox(string title, string message)
        {

        }
        public void ShowDebugMessage(string message)
        {

        }
        private void UpdateUri(MapNavigateMessage msg)
        {
            if(msg.Uri == null) { ViewUri = null; return; }
            ViewUri = msg.Uri;
        }
        private void UpdateHtml(MapHtmlMessage msg)
        {
            if(msg.htmlMessage == null) { ViewHtml = null; return; }
            ViewHtml = msg.htmlMessage;
        }
    }
}
