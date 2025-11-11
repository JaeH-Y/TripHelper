using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Trip.Interfaces.Services
{
    public interface IWindowService
    {
        void Show<TWindow>() where TWindow : Window;
        void Close<TWindow>() where TWindow : Window;
    }
}
