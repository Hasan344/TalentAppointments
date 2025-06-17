using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_subjects")]
    public partial class ExamSubject
    {
        [Column("SubjectId")]
        public int SubjectId { get; set; }

        [Column("ExamId")]
        public int ExamId { get; set; }

        [ForeignKey("SubjectId")]
        [InverseProperty("ExamSubjects")]
        public virtual Subject Subjects { get; set; } = null!;

        [ForeignKey("ExamId")]
        [InverseProperty("ExamSubjects")]
        public virtual Exam Exams { get; set; } = null!;
    }

}