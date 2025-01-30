using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("experts")]
public partial class Expert
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

    [Column("section_id")]
    public int? SectionId { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("Experts")]
    public virtual Section? Section { get; set; }

    [ForeignKey("ExpertId")]
    [InverseProperty("Experts")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [ForeignKey("ExpertId")]
    [InverseProperty("Experts")]
    public virtual ICollection<SubProfession> SubProfessions { get; set; } = new List<SubProfession>();

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
        
    [Column("kons")]
    public Boolean? Kons { get; set; }

    [Column("fin_code")]
    [StringLength(20)]
    public string? FinCode { get; set; }

    [Column("profession")]
    [StringLength(400)]
    public string? Profession { get; set; }
    [Column("gender")]
    public byte? Gender { get; set; }
    [ForeignKey("Gender")]
    [InverseProperty("Experts")]
    public virtual Gender? GenderNavigation { get; set; }

    [Column("serial")]
    [StringLength(15)]
    public string? Serial { get; set; }

    [Column("district")]
    public int? District { get; set; }

    [ForeignKey("District")]
    [InverseProperty("Experts")]
    public virtual District? DistrictNavigation { get; set; }

    [Column("AssignmentCount")]
    public int? AssignmentCount { get; set; }
}
