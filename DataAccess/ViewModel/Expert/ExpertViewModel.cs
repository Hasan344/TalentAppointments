using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ForQab.Data_Access.ViewModel
{
    public class ExpertViewModel
    {
        [Required(ErrorMessage = "Ad boş ola bilməz")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Soyad boş ola bilməz")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "Ata adı boş ola bilməz")]
        public string? Fname { get; set; }

        [Required(ErrorMessage = "İstiqamət seçilməlidir")]
        public int? SectionId { get; set; }

        [Required(ErrorMessage = "Doğum tarixi boş ola bilməz")]
        public DateOnly? BirthDate { get; set; }

        [Required(ErrorMessage = "SSN boş ola bilməz")]
        public string? SSN { get; set; }

        [Required(ErrorMessage = "Rekvizit boş ola bilməz")]
        public string? Rekvizit { get; set; }

        public string? HesablashmaH { get; set; }
        public string? Voen { get; set; }

        [Required(ErrorMessage = "Bank filialı boş ola bilməz")]
        public string? BankFilial { get; set; }

        [Required(ErrorMessage = "Bank filial kodu boş ola bilməz")]
        public string? BankFilialCode { get; set; }

        public Boolean? Kons { get; set; }

        [Required(ErrorMessage = "FİN kod boş ola bilməz")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "FİN kod 6-20 simvol arasında olmalıdır")]
        public string? FinCode { get; set; }

        [Required(ErrorMessage = "Seriya prefiksi boş ola bilməz")]
        public string? SerialPrefix { get; set; }

        [Required(ErrorMessage = "Seriya nömrəsi boş ola bilməz")]
        public string? Serial { get; set; }

        public string? Profession { get; set; }

        [Required(ErrorMessage = "Federasiya seçilməlidir")]
        public int? Federation { get; set; }

        [Required(ErrorMessage = "Cins seçilməlidir")]
        public byte? Gender { get; set; }

        public string? TelIs { get; set; }
        public string? TelEl { get; set; }
        public string? ContractNo { get; set; }
        public DateOnly? ContractDate { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; }
        public int[]? SelectedSubProfessions { get; set; }
    }

    public class SubProfessionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
