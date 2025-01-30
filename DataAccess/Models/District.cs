using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("districts")]
public partial class District
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("region_id")]
    public int? RegionId { get; set; }

    [InverseProperty("DistrictNavigation")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();

    [ForeignKey("RegionId")]
    [InverseProperty("Districts")]
    public virtual Region? Region { get; set; } = null!;
    [InverseProperty("DistrictNavigation")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();
}
