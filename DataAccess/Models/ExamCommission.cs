using ForQab.DataAccess.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("Exam_Commissions")]
public partial class ExamCommission
{
    [Key]
    [Column("Exam_Id")] // Veritabanındaki sütun adıyla eşleşmeli
    public int ExamId { get; set; }

    [Key]
    [Column("Commission_Id")] // Veritabanındaki sütun adıyla eşleşmeli
    public int CommissionId { get; set; }

    [ForeignKey("ExamId")]
    public virtual Exam Exam { get; set; } = null!;

    [ForeignKey("CommissionId")]
    public virtual Commission Commission { get; set; } = null!;
}