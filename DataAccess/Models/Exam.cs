using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [Column("district_id")]
    public int DistrictId { get; set; }

    [Column("exam_bulding_id")]
    public int ExamBuldingId { get; set; }

    [Column("exam_date")]
    public DateOnly ExamDate { get; set; }

    [Column("duration", TypeName = "decimal(10, 1)")]
    public decimal? Duration { get; set; }

    [Column("water")]
    public int? Water { get; set; }

    [Column("food")]
    public int? Food { get; set; }

    [Column("notes")]
    [StringLength(2000)]
    public string? Notes { get; set; }

    [Column("inventory_transport")]
    [StringLength(4000)]
    public string? InventoryTransport { get; set; }

    [ForeignKey("ExamBuldingId")]
    [InverseProperty("Exams")]
    public virtual ExamBuilding? ExamBuilding { get; set; } = null!;

    [ForeignKey("SectionId")]
    [InverseProperty("Exams")]
    public virtual Section? Section { get; set; } = null!;

    [ForeignKey("Type")]
    [InverseProperty("Exams")]
    public virtual ExamType? ExamType { get; set; } = null!;

    [ForeignKey("DistrictId")]
    [InverseProperty("Exams")]
    public virtual District? District { get; set; } = null!;
    public virtual ICollection<ExamCommission> ExamCommissions { get; set; } = new List<ExamCommission>();
    public virtual ICollection<ExamMonitor> ExamMonitors { get; set; } = new List<ExamMonitor>();

    public virtual ICollection<ExamExpertSubProfession> ExamExpertSubProfessions { get; set; } = new List<ExamExpertSubProfession>();
    public virtual ICollection<ExamDegree> ExamDegrees { get; set; } = new List<ExamDegree>();

    [ForeignKey("ExamId")]
    [InverseProperty("Exams")]
    public virtual ICollection<DimRepresentative> Representatives { get; set; } = new List<DimRepresentative>();

    [ForeignKey("ExamId")]
    [InverseProperty("Exams")]
    public virtual ICollection<Expert> Experts { get; set; } = new List<Expert>();

    [ForeignKey("ExamId")]
    [InverseProperty("Exams")]
    public virtual ICollection<Monitor> Monitors { get; set; } = new List<Monitor>();

    [Column("shift")]
    public byte? Shift { get; set; }

    [Column("start_time")]
    public TimeSpan? StartTime { get; set; }

    [Column("end_time")]
    public TimeSpan? EndTime { get; set; }

    [Column("admission_time")]
    public TimeSpan? AdmissionTime { get; set; }

    [Column("student_count")]
    public int? StudentCount { get; set; }

    [Column("Type")]
    public int? Type { get; set; }

    [InverseProperty("Exam")]
    public virtual ICollection<MonitorLog> MonitorLogs { get; set; } = new List<MonitorLog>();
}
