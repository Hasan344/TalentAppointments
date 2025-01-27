using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ForQab.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Models;

[Table("experts_professions")]
public partial class ExpertsProfession
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("expert_id")]
    public int ExpertId { get; set; }

    [Column("sub_profession_id")]
    public int SubProfessionId { get; set; }

    [ForeignKey("ExpertId")]
    [InverseProperty("ExpertsProfessions")]
    public virtual Expert Expert { get; set; } = null!;

    [ForeignKey("SubProfessionId")]
    [InverseProperty("ExpertsProfessions")]
    public virtual SubProfession SubProfession { get; set; } = null!;
}
