using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignExpertForMXToExamViewModel
    {
        [Required]
        public int ExamId { get; set; }

        [Required]
        public int SectionId { get; set; }

        public List<RoomSelectListItem> Rooms { get; set; } = new List<RoomSelectListItem>();

        public List<ExpertFormViewModel> ExpertForms { get; set; } = new List<ExpertFormViewModel>();

        public bool Kons { get; set; }
    }

    public class ExpertFormViewModel
    {
        public bool Kons { get; set; } 
        public int ExpertId { get; set; }
        public int? RoomId { get; set; } 
    }

    // Oda seçimi için gerekli model
    public class RoomSelectListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}