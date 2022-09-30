namespace TimesheetGeneratorApp.Models
{
    public class GenerateCommitModel
    {
        public DateTime tanggal_mulai { set; get; }
        public DateTime tanggal_selesai { set; get; }

        public string btn_generate { set; get; }
        public int project_id { set; get; }

        public string export_type { set; get; }
    }
}
