using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Exam
{
    public class WriteMonitorLogViewModel
    {
        public int ExamId { get; set; }
        public int MonitorId { get; set; }

        [Required(ErrorMessage = "Lütfen qeyd daxil edin.")]
        [StringLength(4000, ErrorMessage = "Qeyd maksimum 4000 simvol ola bilər.")]
        public string? Note { get; set; }
        public string? UserName { get; set; }
        public byte Kind { get; set; } = 0;
        public List<SelectListItem> KindOptions { get; set; } = new List<SelectListItem>();
    }
}
