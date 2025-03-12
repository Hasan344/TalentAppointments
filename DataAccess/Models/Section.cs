using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("sections")]
public partial class Section
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("Section")]
    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();

    [InverseProperty("Section")]
    public virtual ICollection<ExamBuilding> ExamBuildings { get; set; } = new List<ExamBuilding>();

    [InverseProperty("Section")]
    public virtual ICollection<ExamRoom> ExamRooms { get; set; } = new List<ExamRoom>();

    [InverseProperty("Section")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Section")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();

    [InverseProperty("Section")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();

    [InverseProperty("Section")]
    public virtual ICollection<Profession> Professions { get; set; } = new List<Profession>();

    [InverseProperty("Section")]
    public virtual ICollection<SubCommission> SubCommissions { get; set; } = new List<SubCommission>();

    [InverseProperty("Section")]
    public virtual ICollection<SubProfession> SubProfessions { get; set; } = new List<SubProfession>();

    [InverseProperty("Section")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
