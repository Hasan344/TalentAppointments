using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("exam_building")]
public partial class ExamBuilding
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!; 
    
    [Column("code")]
    [StringLength(50)]
    public string? Code { get; set; } 

    [Column("address")]
    [StringLength(350)]
    public string? Address { get; set; }
    
    [Column("section_id")]
    public int SectionId { get; set; }

    [InverseProperty("ExamBuilding")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [ForeignKey("SectionId")]
    [InverseProperty("ExamBuildings")]
    public virtual Section? Section { get; set; } 
}
