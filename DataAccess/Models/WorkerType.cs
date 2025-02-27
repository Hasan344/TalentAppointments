using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("worker_types")]
public partial class WorkerType
{
    [Key]
    [Column("id")]
    public byte Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("WorkerTypeNavigation")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();
}
