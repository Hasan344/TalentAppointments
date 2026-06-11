using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.Presentation.ViewModels
{
    public class CreateExamViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }

        [Required]
        public int DistrictId { get; set; }

        [Required]
        public int ExamBuldingId { get; set; }

        [Required]
        public DateOnly ExamDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 1)")]
        public decimal Duration { get; set; }

        [Required]
        public int Water { get; set; }

        [Required]
        public int Food { get; set; }

        [Required]
        public int? StudentCount { get; set; }
        [Required]
        public int? Type { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
        public int? burQ { get; set; }
        public int? burK { get; set; }

        [StringLength(4000)]
        public string? InventoryTransport { get; set; }
        public List<SelectListItem>? Commissions { get; set; } 
        public int[]? SelectedCommissions { get; set; }
        public List<SelectListItem>? Degrees { get; set; }
        public int[]? SelectedDegrees { get; set; }
        public List<SelectListItem>? Subjects { get; set; }
        public int[]? SelectedSubjects { get; set; }
        public byte? Shift { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public TimeSpan? AdmissionTime { get; set; }
        public int? Stekan { get; set; }
    }
}