using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("expert_logs")]
public partial class ExpertLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("expert_id")]
    public int ExpertId { get; set; }

    [Column("note")]
    [StringLength(4000)]
    public string? Note { get; set; }

    [ForeignKey("ExpertId")]
    [InverseProperty("ExpertLogs")]
    public virtual Expert Expert { get; set; } = null!;

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
    [InverseProperty("ExpertLogs")]
    public virtual Exam Exam { get; set; } = null!;
}
