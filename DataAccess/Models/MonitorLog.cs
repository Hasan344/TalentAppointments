using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("monitor_logs")]
public partial class MonitorLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("supervisor_id")]
    public int SupervisorId { get; set; }

    [Column("note")]
    [StringLength(4000)]
    public string? Note { get; set; }

    [ForeignKey("SupervisorId")]
    [InverseProperty("MonitorLogs")]
    public virtual Monitor Supervisor { get; set; } = null!;

    [Column("kind")]
    public byte Kind { get; set; }

    [Column("time")]
    public DateTime? Time { get; set; }

    [Column("user_name")]
    [StringLength(150)]
    public string? UserName { get; set; }

    [Column("exam_id")]
    public int ExamId { get; set; }

    [ForeignKey("ExamId")]
    [InverseProperty("MonitorLogs")]
    public virtual Exam Exam { get; set; } = null!;

}
