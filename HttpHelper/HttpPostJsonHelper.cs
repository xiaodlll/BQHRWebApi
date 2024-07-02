using System.Text;

namespace BQHRWebApi
{
    public class HttpPostJsonHelper
    {

        private static string _apiUrl;

        public static string ApiUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_apiUrl))
                {
                    var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
           .Build();

                    _apiUrl = configuration["HRApiUrl"];
                }
                return _apiUrl;
            }
        }

        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<string> PostJsonAsync(string jsonContent)
        {
            string path = "api/services";
            Uri baseUri = new Uri(ApiUrl);
            Uri combinedUri = new Uri(baseUri, path);
            string url = combinedUri.ToString();
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}