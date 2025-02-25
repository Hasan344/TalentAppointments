using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("professions")]
public partial class Profession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int SectionId { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("Professions")]
    public virtual Section Section { get; set; } = null!;

    [InverseProperty("Profession")]
    public virtual ICollection<SubProfession> SubProfessions { get; set; } = new List<SubProfession>();
    [InverseProperty("FederationNavigation")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();
    public virtual ICollection<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; } = new List<ExamExpertSubProfession>();
}
