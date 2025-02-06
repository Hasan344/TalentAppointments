using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class EditExamViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }

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

        [StringLength(2000)]
        public string? Notes { get; set; }

        [StringLength(4000)]
        public string? InventoryTransport { get; set; }
        public List<SelectListItem>? Commissions { get; set; }
        public int[]? SelectedCommissions { get; set; }
        public byte? Shift { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
