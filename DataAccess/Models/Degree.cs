using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("degrees")]
public partial class Degree
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;
    public virtual ICollection<ExamDegree> ExamDegrees { get; set; } = new List<ExamDegree>();
}
