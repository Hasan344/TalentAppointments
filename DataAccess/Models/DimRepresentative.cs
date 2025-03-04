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

    [ForeignKey("RepresentativeId")]
    [InverseProperty("Representatives")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}