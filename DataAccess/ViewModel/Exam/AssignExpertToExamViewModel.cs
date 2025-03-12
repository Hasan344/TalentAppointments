using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignExpertToExamViewModel
    {
        public int ExamId { get; set; }
        public int NumberOfExperts { get; set; }
        public int SectionId { get; set; }
        public List<SelectListItem>? Rooms { get; set; }
        public int? RoomId { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; }
        public int[]? SelectedSubProfessions { get; set; }
        public List<SelectListItem>? Federations { get; set; } 
        public int SelectedFederation { get; set; }
        public int[]? SelectedFederations { get; set; }
        public List<ExpertAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class ExpertAssignmentViewModel
    {
        public int NumberOfExperts { get; set; }
        public int[] SelectedSubProfessions { get; set; }
        public int FederationId { get; set; }
        public int[]? SelectedFederations { get; set; }
        public int? RoomId { get; set; }
    }

}