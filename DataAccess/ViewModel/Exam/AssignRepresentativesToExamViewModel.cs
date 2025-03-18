

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignRepresentativesToExamViewModel
    {
        public int ExamId { get; set; }
        public List<int> SelectedRepresentativeIds { get; set; } = new();
        public List<RepresentativeViewModelForAssign> Representatives { get; set; } = new();
    }

    public class RepresentativeViewModelForAssign
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FinCode { get; set; } = string.Empty;
    }
}

