using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("monitor_logs")]
public partial class MonitorLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("supervisor_id")]
    public int SupervisorId { get; set; }

    [Column("note")]
    [StringLength(4000)]
    public string? Note { get; set; }

    [ForeignKey("SupervisorId")]
    [InverseProperty("MonitorLogs")]
    public virtual Monitor Supervisor { get; set; } = null!;
}
