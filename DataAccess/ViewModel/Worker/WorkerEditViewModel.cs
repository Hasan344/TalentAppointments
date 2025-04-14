using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Worker
{
    public class WorkerEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad boş ola bilməz")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad boş ola bilməz")]
        public string Surname { get; set; }

        public string? Fname { get; set; }
        public string? Region { get; set; }

        [Required(ErrorMessage = "İstiqamət seçilməlidir")]
        public int? SectionId { get; set; }
        public List<SelectListItem>? Sections { get; set; }

        [Required(ErrorMessage = "İşçi tipi seçilməlidir")]
        public byte? WorkerType { get; set; }
        public List<SelectListItem>? WorkerTypes { get; set; }

        [Required(ErrorMessage = "İmtahan binası seçilməlidir")]
        public int? ExamBuilding { get; set; }
        public List<SelectListItem>? ExamBuildings { get; set; }

        public byte? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }
        public string? TelIs { get; set; }
        public string? SSN { get; set; }
        public string? Rekvizit { get; set; }
        public string? Voen { get; set; }
        public string? BankFilial { get; set; }
        public string? BankFilialCode { get; set; }

        [Required(ErrorMessage = "Rayon seçilməlidir")]
        public int? District { get; set; }
        public List<SelectListItem>? Districts { get; set; }
    }
}
