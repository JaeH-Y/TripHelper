using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Trip.Interfaces;
using Trip.Interfaces.Services;

namespace Trip.Services
{
    public class ServerLoadingService : IServerLoadingService
    {
        private readonly HttpClient _http;
        public ServerLoadingService(HttpClient http)
        {
            _http = http;
        }
        public async Task<string> GetServerStatus()
        {
            string status = "";
            try
            {
                HttpResponseMessage respone = await _http.GetAsync("api/status");

                respone.EnsureSuccessStatusCode();

                string json = await respone.Content.ReadAsStringAsync();

                return status = "연결 성공!!";
            }
            catch (HttpRequestException ex)
            {
                return status = "연결 지연중..\n"/* + ex.ToString()*/;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return status = $"기타오류...\n {ex}";
            }

        }

    }
}
