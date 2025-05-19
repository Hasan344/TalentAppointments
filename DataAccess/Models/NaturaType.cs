using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("natura_types")]
public partial class NaturaType
{
    [Key]
    [Column("id")]
    public byte Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("NaturaTypeNavigation")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();
}

