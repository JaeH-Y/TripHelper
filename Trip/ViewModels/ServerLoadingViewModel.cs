using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Trip.Interfaces.Services;
using Trip.Views;

namespace Trip.ViewModels
{
    public class ServerLoadingViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceP;
        private readonly IServerLoadingService _loadingS;
        private readonly IWindowService _windowS;
        private string _serverStatusT = "서버에 연결 중입니다.";

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public string ServerStatusT
        {
            get => _serverStatusT;
            set
            {
                SetProperty(ref _serverStatusT, value);
            }
        }
        private string _connectT = "";
        public string ConnectT
        {
            get => _connectT;
            set
            {
                SetProperty(ref _connectT, value);
            }
        }
        private bool _isConncet;
        public bool IsConnect
        {
            get => _isConncet;
            set => SetProperty(ref _isConncet, value);
        }
        private Visibility _isConnected = Visibility.Collapsed;
        public Visibility IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
        private DateTime startTime;
        private DateTime endTime;
        private DispatcherTimer _connecttimer = new DispatcherTimer();
        public ServerLoadingViewModel(IServiceProvider serviceP)
        {
            _serviceP = serviceP;
            _loadingS = _serviceP.GetRequiredService<IServerLoadingService>();
            _windowS = _serviceP.GetRequiredService<IWindowService>();

            ConnectCommand = new RelayCommand(StartClient);

            StartConnectTimer();
            _serviceP = serviceP;
        }

        public ICommand ConnectCommand { get; set; }
        private async void GetServerStatus()
        {
            await _semaphore.WaitAsync();
            try
            {
                ConnectT = await _loadingS.GetServerStatus();

                IsConnected = ConnectT.Equals("연결 성공!!") ? Visibility.Visible : Visibility.Collapsed;
                IsConnect = IsConnected == Visibility.Visible ? true : false;
                if (IsConnect)
                {
                    StopConnectTimer();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void StartClient()
        {
            StopConnectTimer();

            var window = _serviceP.GetRequiredService<MainWindow>();
            if (window != null)
            {
                window.Show();
                Application.Current.MainWindow = window;
            }

            _windowS.Close<ServerLoadingView>();

        }

        private void StopConnectTimer()
        {
            if (_connecttimer != null)
            {
                _connecttimer.Tick -= CheckConnectStatus;
                _connecttimer.Stop();
                _connecttimer = null;
            }
        }
        private void StartConnectTimer()
        {
            startTime = DateTime.Now;
            if(_connecttimer != null)
            {
                StopConnectTimer();
            }
            _connecttimer = new DispatcherTimer();
            _connecttimer.Interval = TimeSpan.FromMilliseconds(500);
            _connecttimer.Tick += CheckConnectStatus;
            _connecttimer.Start();
        }

        private void CheckConnectStatus(object? sender, EventArgs e)
        {
            endTime = DateTime.Now;
            if((endTime - startTime).TotalSeconds > 60)
            {
                ServerStatusT = "서버 연결에 실패했습니다.\r\n서버가 열려있는지 확인하세요.";
                StopConnectTimer();
                return;
            }

            if(ServerStatusT.Equals("서버에 연결 중입니다..."))
            {
                ServerStatusT = "서버에 연결 중입니다.";
            }
            else
            {
                string addT = ".";
                ServerStatusT += addT;
            }
            GetServerStatus();
        }
    }
}
