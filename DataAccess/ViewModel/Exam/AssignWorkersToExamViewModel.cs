using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignWorkersToExamViewModel
    {

        public int ExamId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public List<WorkerAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class WorkerAssignmentViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "At least 1 monitor must be assigned.")]
        public int NumberOfMonitors { get; set; }
        public byte WorkerType { get; set; }
    }
}

