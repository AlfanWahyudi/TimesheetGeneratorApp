namespace TimesheetGeneratorApp.Models
{
    public class CommitModel
    {
        public int Id { get; set; }
        public string author_name { get; set; }
        public string message { get; set; }
        public DateTime? committed_date { get; set; }
        public DateTime? jam_mulai { get; set; }
        public DateTime? jam_akhir { get; set; }
        public int? jumlah_jam { get; set; }

    }
}
