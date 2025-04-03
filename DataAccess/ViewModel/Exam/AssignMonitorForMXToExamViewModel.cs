using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class AssignMonitorForMXToExamViewModel
    {
        [Required]
        public int ExamId { get; set; }

        [Required]
        public int SectionId { get; set; }

        public List<RoomSelectListItem> Rooms { get; set; } = new List<RoomSelectListItem>();

        public List<MonitorFormViewModel> MonitorForms { get; set; } = new List<MonitorFormViewModel>();
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();

    }

    public class MonitorFormViewModel
    {
        public int Role { get; set; }
        public int MonitorId { get; set; }
        public int? RoomId { get; set; } 
    }

}