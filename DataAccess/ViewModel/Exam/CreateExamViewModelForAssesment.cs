using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.Presentation.ViewModels
{
    public class CreateExamViewModelForAssesment
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
        public byte? Shift { get; set; }
        [Required]
        public int? Type { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}