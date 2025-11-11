using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Trip.Interfaces.Services;

namespace Trip.Services
{
    public class ShowDialogService : IShowDialogService
    {
        public void NotReturnMessageBoxShow(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public MessageBoxResult ReturnMessageBoxShow(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
        public void ShowDebugMessage(string message)
        {

        }
    }
}
