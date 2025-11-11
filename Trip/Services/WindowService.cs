using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Trip.Interfaces.Services;

namespace Trip.Services
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceP;

        public WindowService(IServiceProvider serviceP)
        {
            _serviceP = serviceP;
        }

        public void Show<Twindow>() where Twindow : Window
        {
            var window = _serviceP.GetRequiredService<Twindow>();
        }

        public void Close<TWindow>() where TWindow : Window
        {
            foreach(Window w in Application.Current.Windows)
            {
                if(w is TWindow window)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
