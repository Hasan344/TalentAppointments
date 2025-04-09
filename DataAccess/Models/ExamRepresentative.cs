using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_representatives")]
    public partial class ExamRepresentative
    {
        [Key]
        [Column("RepresentativeId")]
        public int RepresentativeId { get; set; }

        [Key]
        [Column("ExamId")]
        public int ExamId { get; set; }

        [ForeignKey("RepresentativeId")]
        [InverseProperty("ExamRepresentatives")]
        public virtual DimRepresentative Representative { get; set; } = null!;


        [ForeignKey("ExamId")]
        [InverseProperty("ExamRepresentatives")]
        public virtual Exam Exam { get; set; } = null!;

    }

}