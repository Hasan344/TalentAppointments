using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.Models
{
    [Table("Exam_Expert_SubProfessions")]
    public class ExamExpertSubProfession
    {
        [Key]
        [Column("Exam_Id")]
        public int ExamId { get; set; }

        [Key]
        [Column("Expert_Id")]
        public int ExpertId { get; set; }

        [Key]
        [Column("SubProfession_Id")]
        public int SubProfessionId { get; set; }

        [Key]
        [Column("Federation_Id")]
        public int FederationId { get; set; }

        [Column("Room_Id")]
        public int? RoomId { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; } = null!;

        [ForeignKey("ExpertId")]
        public virtual Expert Expert { get; set; } = null!;

        [ForeignKey("SubProfessionId")]
        public virtual SubProfession SubProfession { get; set; } = null!;

        [ForeignKey("FederationId")]
        public virtual Profession Federation { get; set; } = null!;

        [ForeignKey("RoomId")]
        public virtual ExamRoom ExamRoom { get; set; } = null!;
    }

}
