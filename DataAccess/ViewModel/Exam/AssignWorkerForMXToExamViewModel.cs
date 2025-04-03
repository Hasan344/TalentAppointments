using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignWorkerForMXToExamViewModel
    {
        [Required]
        public int ExamId { get; set; }

        [Required]
        public int SectionId { get; set; }

        public List<MonitorFormViewModel> MonitorForms { get; set; } = new List<MonitorFormViewModel>();
        public List<SelectListItem> WorkerTypes { get; set; } = new List<SelectListItem>();
    }

    public class WorkerFormViewModel
    {
        public int WorkerType { get; set; }
        public int[] SelectedWorkerTypes { get; set; }
        public int MonitorId { get; set; }
    }

}