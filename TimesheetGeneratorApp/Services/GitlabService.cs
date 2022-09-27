using Newtonsoft.Json;
using System.Collections;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Services
{
    public class GitlabService
    {
        public string url;
        private HttpClient client;

        public GitlabService(HttpClient httpClient)
        {
            this.client = httpClient;
            this.url = "/api/v4/projects/";
        }

        public ArrayList getList(string hostUrl, string projectId, string access_token, string since, string until, string all, string with_stats, string per_page)
        {
            ArrayList data = new ArrayList();
            HttpResponseMessage response;
            string responseMessage;

            var totalPages = this.getTotalPages(hostUrl, projectId, access_token, since, until, all, with_stats, per_page);

            if (totalPages != 0)
            {
                var iter = 1;
                while (iter <= totalPages)
                {
                    response = client.GetAsync(hostUrl + this.url + projectId + "/repository/commits?access_token=" + access_token
                                    + "&" + "since=" + since
                                    + "&" + "until=" + until
                                    + "&" + "all=" + all
                                    + "&" + "with_stats=" + with_stats
                                    + "&" + "per_page=" + per_page
                                    + "&" + "page=" + iter).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var res = response.Content;
                        string rawData = res.ReadAsStringAsync().Result;

                        List<GitlabCommitModel> dataResponse = JsonConvert.DeserializeObject<List<GitlabCommitModel>>(rawData);

                        foreach (var item in dataResponse)
                        {
                            data.Add(item);
                        }
                    }

                    responseMessage = response.Content.ReadAsStringAsync().Result;
                    iter++;
                }

                return data;
            }

            throw new HttpRequestException("Ada Error Nih");

        }

        public int getTotalPages(string hostUrl, string projectId, string access_token, string since, string until, string all, string with_stats, string per_page)
        {
            var response = client.GetAsync(hostUrl + this.url + projectId + "/repository/commits?access_token=" + access_token
                                    + "&" + "since=" + since
                                    + "&" + "until=" + until
                                    + "&" + "all=" + all
                                    + "&" + "with_stats=" + with_stats
                                    + "&" + "per_page=" + per_page).Result;

            var responseMessage = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                var res = response.Content;
                string rawData = res.ReadAsStringAsync().Result;

                if (response.Headers.Contains("X-Total-Pages")) return int.Parse(response.Headers.GetValues("X-Total-Pages").First());
            }

            throw new HttpRequestException(responseMessage);

        }
    }
}
