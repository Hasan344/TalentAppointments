using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignExpertToExamViewModel
    {
        public int ExamId { get; set; }

       // [Range(1, int.MaxValue, ErrorMessage = "The number of experts must be at least 1.")]
        public int NumberOfExperts { get; set; }

        public int SectionId { get; set; }

        public List<SelectListItem>? SubProfessions { get; set; } // For dropdown list
        public int[]? SelectedSubProfessions { get; set; } // IDs of selected SubProfessions

        public List<ExpertAssignmentViewModel> Assignments { get; set; } = new(); // Yeni uzman ekleme için
    }

    public class ExpertAssignmentViewModel
    {
        public int NumberOfExperts { get; set; }
        public int[] SelectedSubProfessions { get; set; }
    }

}