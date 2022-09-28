using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string jam_mulai { get; set; }
        [DisplayName("Jam aKhir")]
        public string jam_akhir { get; set; }
        [DisplayName("Jumlah jam")]
        public int? jumlah_jam { get; set; }

        [ForeignKey("MasterProjectModel")]
        public int MasterProjectModelId { get; set; }

        public MasterProjectModel? MasterProjectModel { get; set; }
    }
}
