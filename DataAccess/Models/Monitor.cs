using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Table("monitors")]
public partial class Monitor
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

    [Column("region")]
    [StringLength(100)]
    public string? Region { get; set; }

    [Column("section_id")]
    public int? SectionId { get; set; }

    [Column("archive")]
    public byte Archive { get; set; }

    [Column("gender")]
    public byte? Gender { get; set; }

    [Column("role")]
    public byte? Role { get; set; }

    [Column("v_num")]
    public string? VNum { get; set; }

    [Column("profession")]
    [StringLength(50)]
    public string? Profession { get; set; }

    [Column("workplace")]
    [StringLength(50)]
    public string? Workplace { get; set; }

    [Column("position")]
    [StringLength(50)]
    public string? Position { get; set; }

    [Column("age")]
    public int? Age { get; set; }

    [Column("foto")]
    public int? Foto { get; set; }

    [Column("tel_ev")]
    [StringLength(12)]
    public string? TelEv { get; set; }

    [Column("tel_is")]
    [StringLength(12)]
    public string? TelIs { get; set; }

    [Column("fin_code")]
    [StringLength(10)]
    public string? FinCode { get; set; }

    [Column("serial")]
    [StringLength(15)]
    public string? Serial { get; set; }

    [Column("district")]
    public int? District { get; set; }

    [ForeignKey("District")]
    [InverseProperty("Monitors")]
    public virtual District? DistrictNavigation { get; set; }

    [ForeignKey("Gender")]
    [InverseProperty("Monitors")]
    public virtual Gender? GenderNavigation { get; set; }

    [InverseProperty("Supervisor")]
    public virtual ICollection<MonitorLog> MonitorLogs { get; set; } = new List<MonitorLog>();

    [ForeignKey("Role")]
    [InverseProperty("Monitors")]
    public virtual Role? RoleNavigation { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("Monitors")]
    public virtual Section? Section { get; set; }

    [ForeignKey("MonitorId")]
    [InverseProperty("Monitors")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>(); 
    [Column("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column("ssn")]
    [StringLength(400)]
    public string? SSN { get; set; }

    [Column("rekvizit")]
    [StringLength(400)]
    public string? Rekvizit { get; set; }

    [Column("hesablashma_h")]
    [StringLength(400)]
    public string? HesablashmaH { get; set; }

    [Column("voen")]
    [StringLength(400)]
    public string? Voen { get; set; }

    [Column("bank_filial")]
    [StringLength(400)]
    public string? BankFilial { get; set; }

    [Column("bank_filial_code")]
    [StringLength(400)]
    public string? BankFilialCode { get; set; }
}
