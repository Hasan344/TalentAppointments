using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignMonitorsToExamViewModel
    {
        public int ExamId { get; set; }
        public int SectionId { get; set; }
        public List<SelectListItem>? Rooms { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; }
        public int? selectedRoom { get; set; }
        [Required]
        public List<MonitorAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class MonitorAssignmentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "At least 1 monitor must be assigned.")]
        public int NumberOfMonitors { get; set; }
        public int? GenderId { get; set; }
        public DateOnly? MaxDate { get; set; }
        public int? RoomId { get; set; }
        public int? SubProfessionId { get; set; }
        public bool IsReserve { get; set; }
    }
}
