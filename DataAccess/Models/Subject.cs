using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("subjects")]
public partial class Subject
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int? SectionId { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("Subjects")]
    public virtual Section? Section { get; set; } 
    public virtual ICollection<ExamSubject> ExamSubjects { get; set; } = new List<ExamSubject>();
}
