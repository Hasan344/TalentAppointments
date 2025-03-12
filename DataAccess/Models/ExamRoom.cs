using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("exam_rooms")]
    public partial class ExamRoom
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Column("section_id")]
        public int SectionId { get; set; }

        [ForeignKey("SectionId")]
        [InverseProperty("ExamRooms")]
        public virtual Section? Section { get; set; }
        public virtual ICollection<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; } = new List<ExamExpertSubProfession>();
        public virtual ICollection<ExamMonitor> ExamMonitors { get; set; } = new List<ExamMonitor>();
    }
}
