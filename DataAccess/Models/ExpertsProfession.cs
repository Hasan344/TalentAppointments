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
    [Column("expert_id")]
    public int ExpertId { get; set; }
    
    [Key]
    [Column("sub_profession_id")]
    public int SubProfessionId { get; set; }

    [ForeignKey("ExpertId")]
    public virtual Expert Expert { get; set; } = null!;

    [ForeignKey("SubProfessionId")]
    public virtual SubProfession SubProfession { get; set; } = null!;
}

