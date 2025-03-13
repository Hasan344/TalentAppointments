using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_monitors")]
    public partial class ExamMonitor
    {
        [Key]
        [Column("MonitorId")]
        public int MonitorId { get; set; }
        
        [Key]
        [Column("ExamId")]
        public int ExamId { get; set; }

        [Column("RoomId")]
        public int? RoomId { get; set; }

        [ForeignKey("MonitorId")]
        [InverseProperty("ExamMonitors")]
        public virtual Monitor Monitors { get; set; } = null!;

        [ForeignKey("ExamId")]
        [InverseProperty("ExamMonitors")]
        public virtual Exam Exams { get; set; } = null!;

        [ForeignKey("RoomId")]
        [InverseProperty("ExamMonitors")]
        public virtual ExamRoom ExamRooms { get; set; } = null!;
    }
}
