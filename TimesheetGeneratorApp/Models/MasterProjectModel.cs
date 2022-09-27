using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimesheetGeneratorApp.Models
{
    public class MasterProjectModel
    {
        public int id { get; set; }
        [DisplayName("Nama Project")]
        public string name { get; set; }
        [DisplayName("Project ID")]
        public string project_id { get; set; }
        [DisplayName("Version control")]
        public string version_control { get; set; }
        [DisplayName("Host url")]
        public string host_url { get; set; }
        [DisplayName("Akses token")]
        public string? accsess_token { get; set; }
        [DisplayName("Username")]
        public string? username { get; set; }
        [DisplayName("Password")]
        public string? password { get; set; }
    }
}
