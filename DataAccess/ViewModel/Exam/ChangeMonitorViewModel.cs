using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class ChangeMonitorViewModel
    {
        public int ExamId { get; set; }
        public int CurrentMonitorId { get; set; }
        public int NewMonitorId { get; set; }
        public int RoomId { get; set; }
        public List<SelectListItem> AvailableMonitors { get; set; }
    }
}
