using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class ChangeExpertViewModel
    {
        public int ExamId { get; set; }
        public int CurrentExpertId { get; set; }
        public int NewExpertId { get; set; }
        public List<SelectListItem> AvailableExperts { get; set; }
    }
}
