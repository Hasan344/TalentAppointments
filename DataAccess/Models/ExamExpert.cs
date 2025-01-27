using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_experts")]
    public partial class ExamExpert
    {
        [Column("ExpertId")]
        public int ExpertId { get; set; }

        [Column("ExamId")]
        public int ExamId { get; set; }

        [ForeignKey("ExpertId")]
        [InverseProperty("ExamExperts")]
        public virtual Expert Experts { get; set; } = null!;

        [ForeignKey("ExamId")]
        [InverseProperty("ExamExperts")]
        public virtual Exam Exams { get; set; } = null!;
    }

}