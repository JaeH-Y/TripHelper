using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trip.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private string _mainText = "Setting ViewModel Binding Success!!";
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }
    }
}
