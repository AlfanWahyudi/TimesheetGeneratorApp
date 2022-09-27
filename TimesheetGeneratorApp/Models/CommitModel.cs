using System.ComponentModel;

namespace TimesheetGeneratorApp.Models
{
    public class CommitModel
    {
        public int Id { get; set; }
        [DisplayName("Nama author")]
        public string author_name { get; set; }
        [DisplayName("Pesan")]
        public string message { get; set; }
        [DisplayName("Tanggal commit")]
        public DateTime? committed_date { get; set; }
        [DisplayName("Jam mulai")]
        public DateTime? jam_mulai { get; set; }
        [DisplayName("Jam aKhir")]
        public DateTime? jam_akhir { get; set; }
        [DisplayName("Jumlah jam")]
        public int? jumlah_jam { get; set; }

    }
}
