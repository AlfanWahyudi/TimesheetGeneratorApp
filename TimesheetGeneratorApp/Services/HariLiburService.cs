using Newtonsoft.Json;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Services
{
    public class HariLiburService
    {
        public string url;
        public HttpClient httpClient;

        public HariLiburService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.url = "https://api-harilibur.vercel.app/";
        }

        public List<HariLiburModel> getListByYear(string year)
        {
            var response = httpClient.GetAsync(url + "api?year=" + year).Result;
            var responseMessage = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                var res = response.Content;
                string rawData = res.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<List<HariLiburModel>>(rawData);
            }

            throw new HttpRequestException(responseMessage);
        }
    }
}
