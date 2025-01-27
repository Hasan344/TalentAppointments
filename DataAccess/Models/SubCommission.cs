using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("sub_commissions")]
public partial class SubCommission
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sub_commission_no")]
    [StringLength(10)]
    public string SubCommissionNo { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int SectionId { get; set; }

    [Column("commission_id")]
    public int CommissionId { get; set; }

    [ForeignKey("CommissionId")]
    [InverseProperty("SubCommissions")]
    public virtual Commission Commission { get; set; } = null!;

    [InverseProperty("SubCommission")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [ForeignKey("SectionId")]
    [InverseProperty("SubCommissions")]
    public virtual Section Section { get; set; } = null!;
}
