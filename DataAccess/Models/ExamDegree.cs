using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_degrees")]
    public partial class ExamDegree
    {
        [Column("DegreeId")]
        public int DegreeId { get; set; }

        [Column("ExamId")]
        public int ExamId { get; set; }

        [ForeignKey("DegreeId")]
        [InverseProperty("ExamDegrees")]
        public virtual Degree Degrees { get; set; } = null!;

        [ForeignKey("ExamId")]
        [InverseProperty("ExamDegrees")]
        public virtual Exam Exams { get; set; } = null!;
    }

}