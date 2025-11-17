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
using System.IO;
using Trip.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Text.Json;
using Trip.Interfaces;
using Trip.Config;

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

            // Config JSON
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "Config.json");

            var configlList = GetSettingJSON(filePath);
            var config = configlList.First();

            // DI Container
            var services = new ServiceCollection();

            ConfigureServices(services, config, filePath);

            ServiceProvider = services.BuildServiceProvider();

            /*var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {

                mainWindow.Show();
            }*/


            // View Loading
            var serverWindow = ServiceProvider.GetRequiredService<ServerLoadingView>();
            if(serverWindow != null)
            {
                serverWindow.Show();
            }
        }
        private void ConfigureServices(IServiceCollection services, ConfigJSONModel config, string filepath)
        {

            // Config
            services.AddSingleton<IAppConfig>(sp => new AppConfig(config,filepath));

            Console.WriteLine(config.LocalUrl);
            // HttpClient 관리
            services.AddSingleton(new HttpClient()
            {
                BaseAddress = new Uri(config.LocalUrl)
            });

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

            // 주소 여기서 등록해주는거 없애
            services.AddSingleton<IServerLoadingService>(sp => sp.GetRequiredService<ServerLoadingService>());

            // Views
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ServerLoadingView>();
        }

        private List<ConfigJSONModel> GetSettingJSON(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SetDefualtSettingJSON(filePath);
            }

            var json = ReadConfigJSON(filePath);

            if(json == null)
            {
                Console.WriteLine("ReadJSONError");

                // 한번 더 저장(방어)
                json = new List<ConfigJSONModel>
                {
                    new ConfigJSONModel
                    {
                        Mode = 1,
                        LocalUrl = "https://localhost:7174/",
                        HttpUrl = string.Empty,
                        IsTimeRequest = 0,
                    }
                };
                SaveConfigJSON(filePath, json);
            }
            
            return json;

        }

        private void SetDefualtSettingJSON(string filePath)
        {
            try
            {
                // 새 리스트 생성
                List<ConfigJSONModel> list = new List<ConfigJSONModel>();
                // 기본값 부여

                var defualt = new ConfigJSONModel
                {
                    Mode = 1,
                    LocalUrl = "https://localhost:7174/",
                    HttpUrl = string.Empty,
                    IsTimeRequest = 0,
                };

                list.Add(defualt);

                // 저장
                SaveConfigJSON(filePath, list);
            }
            catch (Exception ex)
            {
            }
        }
        private void SaveConfigJSON(string filePath, List<ConfigJSONModel> list)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var newJson = JsonSerializer.Serialize(list, options);
                File.WriteAllText(filePath, newJson);

                Console.WriteLine("Config JSON 저장 성공");
            }
            catch (Exception ex)
            {

            }
        }
        private List<ConfigJSONModel> ReadConfigJSON(string filePath)
        {
            try
            {
                var json = JsonSerializer.Deserialize<List<ConfigJSONModel>>(File.ReadAllText(filePath));

                return json;
            }
            catch
            {
                return null;
            }
        }
    }

}
