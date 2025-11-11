using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Trip.Interfaces.ViewModels;
using Trip.Interfaces.Services;
using Trip.Services;
using Trip.ViewModels;
using Trip.Views;
using CommunityToolkit.Mvvm.Messaging;
using System.Net.Http;

namespace Trip
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            /*var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {

                mainWindow.Show();
            }*/

            var serverWindow = ServiceProvider.GetRequiredService<ServerLoadingView>();
            if(serverWindow != null)
            {
                serverWindow.Show();
            }
        }
        private void ConfigureServices(IServiceCollection services)
        {
            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<NewPlanViewModel>();
            services.AddSingleton<AccommodationViewModel>();
            services.AddSingleton<LikePlaceViewModel>();
            services.AddSingleton<PlanCabinetViewModel>();
            services.AddSingleton<SettingViewModel>();
            services.AddSingleton<ServerLoadingViewModel>();
            // Services
            services.AddSingleton<FavoritePlaceService>();
            services.AddSingleton<ShowDialogService>();
            services.AddSingleton<AccommodationService>();
            services.AddSingleton<PlanManagementService>();
            services.AddSingleton<GoogleAPIService>();
            services.AddSingleton<ServerLoadingService>();
            services.AddSingleton<WindowService>();

            // Messenger
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            // ViewModel <-> Interface
            services.AddSingleton<INewPlanViewModel>(sp => sp.GetRequiredService<NewPlanViewModel>());
            services.AddSingleton<IPlanCabinetViewModel>(sp => sp.GetRequiredService<PlanCabinetViewModel>());

            // Service <-> Interface
            services.AddSingleton<IFavoritePlaceService>(sp => sp.GetRequiredService<FavoritePlaceService>());
            services.AddSingleton<IShowDialogService>(sp => sp.GetRequiredService<ShowDialogService>());
            services.AddSingleton<IAccommodationService>(sp => sp.GetRequiredService<AccommodationService>());
            services.AddSingleton<IPlanManagementService>(sp => sp.GetRequiredService<PlanManagementService>());
            services.AddSingleton<IWindowService>(sp => sp.GetRequiredService<WindowService>());
            services.AddSingleton<IGoogleAPIService>(sp =>
            {
                var http = new HttpClient();
                var key = "AIzaSyDpqNM6pN5Nq67EIZGl9QfjhF6RCBfSA0Y";
                return new GoogleAPIService(key, http);
            });
            services.AddSingleton<IServerLoadingService>(sp =>
            {
                var http = new HttpClient();
                http.BaseAddress = new Uri("https://localhost:7174");
                return new ServerLoadingService(http);
            });

            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ServerLoadingView>();
        }
    }

}
