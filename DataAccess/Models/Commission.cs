using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("commissions")]
public partial class Commission
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("commission_no")]
    [StringLength(10)]
    public string CommissionNo { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int SectionId { get; set; }

    [InverseProperty("Commission")]
    public virtual ICollection<Exam>? Exams { get; set; } = new List<Exam>();

    [ForeignKey("SectionId")]
    [InverseProperty("Commissions")]
    public virtual Section? Section { get; set; } = null!;

    [InverseProperty("Commission")]
    public virtual ICollection<SubCommission>? SubCommissions { get; set; } = new List<SubCommission>();
}
