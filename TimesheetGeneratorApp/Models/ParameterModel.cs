namespace TimesheetGeneratorApp.Models
{
    public class ParameterModel
    {
        public string AccessToken { get; set; }
        public string Since { get; set; }
        public string Until { get; set; }
        public string All { get; set; }
        public string WithStats { get; set; }
        public string PerPage { get; set; }
    }
}