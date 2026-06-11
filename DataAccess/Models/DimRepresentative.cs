using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("dim_representative")]
public partial class DimRepresentative
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("surname")]
    [StringLength(100)]
    public string Surname { get; set; } = null!;

    [Column("fname")]
    [StringLength(100)]
    public string Fname { get; set; } = null!;

    [Column("fin_code")]
    [StringLength(50)]
    public string FinCode { get; set; } = null!;

    [Column("tel")]
    [StringLength(50)]
    public string Tel { get; set; } = null!;

    [Column("serial")]
    [StringLength(50)]
    public string Serial { get; set; } = null!;

    [Column("type")]
    public int Type { get; set; }

    [Column("gender")]
    public int Gender { get; set; }

    [ForeignKey("RepresentativeId")]
    [InverseProperty("Representatives")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>(); 
    [InverseProperty("Representative")]
    public virtual ICollection<ExamRepresentative> ExamRepresentatives { get; set; } = new List<ExamRepresentative>();

    [Column("serial_prefix")]
    [StringLength(10)]
    public string? SerialPrefix { get; set; }

    [Column("photo")]
    [StringLength(4000)]
    public string? Photo { get; set; }
    [Column("archive")]
    public byte Archive { get; set; }

    [Column("archive_reason")]
    [StringLength(2000)]
    public string? ArchiveReason { get; set; }

}