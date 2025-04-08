using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("Exam_Types")]
public partial class ExamType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!; 

    [InverseProperty("ExamType")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
