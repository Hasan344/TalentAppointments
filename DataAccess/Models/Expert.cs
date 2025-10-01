using ForQab.Models;
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
    [Column("tel_is")]
    [StringLength(50)]
    public string? TelIs { get; set; } = null!;

    [Column("tel_el")]
    [StringLength(50)]
    public string? TelEl { get; set; } = null!;

    [Column("section_id")]
    public int? SectionId { get; set; }

    [ForeignKey("SectionId")]
    [InverseProperty("Experts")]
    public virtual Section? Section { get; set; }

    [ForeignKey("ExpertId")]
    [InverseProperty("Experts")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public virtual ICollection<ExpertsProfession> ExpertsProfessions { get; set; } = new List<ExpertsProfession>();

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
    public virtual ICollection<ExamExpert> ExamExperts { get; set; } = new List<ExamExpert>();

    [Column("AssignmentCount")]
    public int? AssignmentCount { get; set; }

    [Column("federation")]
    public int? Federation {  get; set; }

    [ForeignKey("Federation")]
    public virtual Profession? FederationNavigation { get; set; }

    [InverseProperty("Expert")]
    public virtual ICollection<ExpertLog> ExpertLogs { get; set; } = new List<ExpertLog>();

    [Column("archive")]
    public byte Archive { get; set; }

    [Column("archive_reason")]
    [StringLength(2000)]
    public string? ArchiveReason { get; set; }

    [Column("status")]
    public byte? Status { get; set; }

    [Column("status_reason")]
    [StringLength(2000)]
    public string? StatusReason { get; set; }
    public virtual ICollection<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; } = new List<ExamExpertSubProfession>(); 
    public ICollection<SubProfession> SubProfessions { get; set; }

    [Column("photo")]
    [StringLength(4000)]
    public string? Photo { get; set; }

    [Column("contract_no")]
    [StringLength(150)]
    public string? ContractNo { get; set; }

    [Column("contract_date")]
    public DateOnly? ContractDate { get; set; }
    public ICollection<Contract> Contracts { get; set; }
    [NotMapped]
    public int ComputedAssignmentCount => ExamExpertSubProfessions?.Count ?? 0;
    [NotMapped]
    public int ComputedAssignmentCountShift1 => ExamExpertSubProfessions?.Count(e => e.Exam?.Shift == 1) ?? 0;

    [NotMapped]
    public int ComputedAssignmentCountShift2 => ExamExpertSubProfessions?.Count(e => e.Exam?.Shift == 2) ?? 0;

    [Column("serial_prefix")]
    [StringLength(10)]
    public string? SerialPrefix { get; set; }

}
