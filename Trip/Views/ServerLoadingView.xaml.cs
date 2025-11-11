using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Trip.ViewModels;

namespace Trip.Views
{
    /// <summary>
    /// ServerLoadingView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ServerLoadingView : Window
    {
        private readonly IServiceProvider _serviceP;
        private ServerLoadingViewModel _viewModel;
        public ServerLoadingView(IServiceProvider serviceP)
        {
            InitializeComponent();
            _serviceP = serviceP;

            _viewModel = _serviceP.GetRequiredService<ServerLoadingViewModel>();
            DataContext = _viewModel;
        }
    }
}
