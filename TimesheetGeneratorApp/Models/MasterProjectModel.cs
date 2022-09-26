using System;

namespace TimesheetGeneratorApp.Models
{
    public class MasterProjectModel
    {
        public int id { get; set; }

        public string name { get; set; }

        public string project_id { get; set; }
        public string version_control { get; set; }
        public string host_url { get; set; }
        public string? accsess_token { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
    }
}
