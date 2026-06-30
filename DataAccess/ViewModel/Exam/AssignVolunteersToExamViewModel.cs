

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignVolunteersToExamViewModel
    {
        public int ExamId { get; set; }
        public List<int> SelectedVolunteerIds { get; set; } = new();
        public List<VolunteerViewModelForAssign> Volunteers { get; set; } = new();
    }

    public class VolunteerViewModelForAssign
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Fname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FinCode { get; set; } = string.Empty;
    }
}

