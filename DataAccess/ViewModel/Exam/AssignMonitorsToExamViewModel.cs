using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignMonitorsToExamViewModel
    {
        public int ExamId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The number of experts must be at least 1.")]
        public int NumberOfMonitors { get; set; }
        public int SectionId { get; set; }
    }
}
