using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string Number { get; set; }
        [Column("Contract_Date")]
        public DateTime Date { get; set; }

        [Column("Expert_Id")]
        public int? ExpertId { get; set; }
        public Expert? Expert { get; set; }

        [Column("Monitor_Id")]
        public int? MonitorId { get; set; }
        public Monitor? Monitor { get; set; }
    }
}
