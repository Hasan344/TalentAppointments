using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("sub_professions")]
public partial class SubProfession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int? SectionId { get; set; }

    [Column("profession_id")]
    public int? ProfessionId { get; set; }

    [ForeignKey("ProfessionId")]
    [InverseProperty("SubProfessions")]
    public virtual Profession? Profession { get; set; } 

    [ForeignKey("SectionId")]
    [InverseProperty("SubProfessions")]
    public virtual Section? Section { get; set; } = null!;

    [ForeignKey("SubProfessionId")]
    [InverseProperty("SubProfessions")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();
    public virtual ICollection<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; } = new List<ExamExpertSubProfession>();
}
