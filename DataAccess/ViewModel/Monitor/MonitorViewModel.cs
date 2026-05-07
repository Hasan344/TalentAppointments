using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.DataAccess.ViewModel.Monitor
{
    public class MonitorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad boş ola bilməz")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad boş ola bilməz")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Ata adı boş ola bilməz")]
        public string? Fname { get; set; }

        public string? Region { get; set; }

        [Required(ErrorMessage = "FİN kod boş ola bilməz")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "FİN kod 6-10 simvol arasında olmalıdır")]
        public string? FinCode { get; set; }

        [Required(ErrorMessage = "Seriya prefiksi boş ola bilməz")]
        public string? SerialPrefix { get; set; }

        [Required(ErrorMessage = "Seriya nömrəsi boş ola bilməz")]
        public string? Serial { get; set; }

        [Required(ErrorMessage = "İstiqamət seçilməlidir")]
        public int SectionId { get; set; }
        public List<SelectListItem>? Sections { get; set; }

        public byte? Gender { get; set; }

        [Required(ErrorMessage = "Doğum tarixi boş ola bilməz")]
        public DateOnly? BirthDate { get; set; }

        public string? ContractNo { get; set; }
        public DateOnly? ContractDate { get; set; }
        public string? Uni { get; set; }
        public string? Position { get; set; }
        public string? Profession { get; set; }

        [Required(ErrorMessage = "SSN boş ola bilməz")]
        public string? SSN { get; set; }

        [Required(ErrorMessage = "Rekvizit boş ola bilməz")]
        public string? Rekvizit { get; set; }

        public string? Voen { get; set; }

        [Required(ErrorMessage = "Bank filialı boş ola bilməz")]
        public string? BankFilial { get; set; }

        [Required(ErrorMessage = "Bank filial kodu boş ola bilməz")]
        public string? BankFilialCode { get; set; }

        public int? Role { get; set; }
        public int? Status { get; set; }
        public int? AssignmentCount { get; set; }
        public int? ThisYearAssignmentCount { get; set; }
        public int? Archive { get; set; }
        public string? VNum { get; set; }
        public string? Workplace { get; set; }
        public string? HesablashmaH { get; set; }
        public string? TelIs { get; set; }


        [Required(ErrorMessage = "Rayon seçilməlidir")]
        public byte District { get; set; }
        public List<SelectListItem>? Districts { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; }
        public int[]? SelectedSubProfessions { get; set; }
    }
}
