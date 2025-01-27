using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_monitors")]
    public partial class ExamMonitor
    {
        [Column("MonitorId")]
        public int MonitorId { get; set; }

        [Column("ExamId")]
        public int ExamId { get; set; }

        [ForeignKey("MonitorId")]
        [InverseProperty("ExamMonitors")]
        public virtual Monitor Monitors { get; set; } = null!;

        [ForeignKey("ExamId")]
        [InverseProperty("ExamMonitors")]
        public virtual Exam Exams { get; set; } = null!;
    }
}
