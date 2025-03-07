using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class ChangeRepresentativeViewModel
    {
        public int ExamId { get; set; }
        public int CurrentRepresentativeId { get; set; }
        public int NewRepresentativeId { get; set; }
        public List<SelectListItem> AvailableRepresentatives { get; set; }
    }
}
