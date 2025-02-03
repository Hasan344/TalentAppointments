using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("exams")]
public partial class Exam
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("section_id")]
    public int SectionId { get; set; }

    [Column("exam_bulding_id")]
    public int ExamBuldingId { get; set; }

    [Column("exam_date")]
    public DateOnly ExamDate { get; set; }

    [Column("duration", TypeName = "decimal(10, 1)")]
    public decimal Duration { get; set; }

    [Column("water")]
    public int Water { get; set; }

    [Column("food")]
    public int Food { get; set; }

    [Column("notes")]
    [StringLength(2000)]
    public string? Notes { get; set; }

    [Column("inventory_transport")]
    [StringLength(4000)]
    public string? InventoryTransport { get; set; }

    [ForeignKey("ExamBuldingId")]
    [InverseProperty("Exams")]
    public virtual ExamBuilding? ExamBulding { get; set; } = null!;

    [ForeignKey("SectionId")]
    [InverseProperty("Exams")]
    public virtual Section? Section { get; set; } = null!;

    [ForeignKey("Exam_Id")]
    [InverseProperty("Exams")]
    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    [ForeignKey("ExamId")]
    [InverseProperty("Exams")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();

    [ForeignKey("ExamId")]
    [InverseProperty("Exams")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();
}
