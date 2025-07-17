using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class EditExamViewModelForAssesment
    {
        public int Id { get; set; }

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
        public byte? Shift { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
