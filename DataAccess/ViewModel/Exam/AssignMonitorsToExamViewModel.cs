using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignMonitorsToExamViewModel
    {
        public int ExamId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public List<MonitorAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class MonitorAssignmentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "At least 1 monitor must be assigned.")]
        public int NumberOfMonitors { get; set; }
        public int GenderId { get; set; }
        public DateOnly MaxDate { get; set; }
        public int? RoomId { get; set; }
    }
}
