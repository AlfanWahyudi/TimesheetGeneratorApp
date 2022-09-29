using System.ComponentModel.DataAnnotations;

namespace TimesheetGeneratorApp.Models
{
    public class HariLiburModel
    {
        [Key]
        public int Id { get; set; }
        public DateTime? holiday_date { get; set; }
        public string holiday_name { get; set; }
        public bool is_national_holiday { get; set; }   
    }
}