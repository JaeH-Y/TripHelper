using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Trip.ViewModels
{
    public class LogInViewModel : ViewModelBase
    {

        private string _idText;
        public string IDText
        {
            get => _idText;
            set => SetProperty(ref _idText, value);
        }

        private string _pwText;
        public string PWText
        {
            get => _pwText;
            set => SetProperty(ref _pwText, value);
        }
        public ICommand LogInCommand { get; set; }
        public ICommand FindAccountCommand { get; set; }
        public LogInViewModel()
        {
            CommandInit();
        }
        private void CommandInit()
        {
            LogInCommand = new AsyncRelayCommand(RequestLogIn);
            FindAccountCommand = new AsyncRelayCommand(RequestFindAccount);
        }

        private async Task RequestLogIn()
        {

        }
        private async Task RequestFindAccount()
        {

        }




    }
}
