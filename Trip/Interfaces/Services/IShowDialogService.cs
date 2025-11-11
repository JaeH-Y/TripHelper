using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Trip.Interfaces.Services
{
    public interface IShowDialogService
    {
        void NotReturnMessageBoxShow(string title, string message);
        MessageBoxResult ReturnMessageBoxShow(string title, string message);
        void ShowDebugMessage(string message);
    }
}
