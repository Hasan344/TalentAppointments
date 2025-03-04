using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignWorkersToExamViewModel
    {

        public int ExamId { get; set; }
        public int SectionId { get; set; }
        public List<int> SelectedWorkerIds { get; set; }
        [Required]
        public List<WorkerAssignmentViewModel> Assignments { get; set; } = new();
    }

    public class WorkerAssignmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FinCode { get; set; } = string.Empty;
        public string WorkerType { get; set; }
    }
}

