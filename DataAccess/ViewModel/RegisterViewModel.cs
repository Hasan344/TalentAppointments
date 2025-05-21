using System.ComponentModel.DataAnnotations;

namespace ForQab.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "İstifadəçi adı yazılmalıdır")]
        [DataType(DataType.Text)]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Ad yazılmalıdır")]
        [DataType(DataType.Text)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Soyad yazılmalıdır")]
        [DataType(DataType.Text)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Parol yazılmalıdır")]
        [StringLength(36, ErrorMessage = "Ölçü {2} və {1} aralığında olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Parol yazılmalıdır")]
        [DataType(DataType.Password)]
        [StringLength(36, ErrorMessage = "Ölçü {2} və {1} aralığında olmalıdır.", MinimumLength = 6)]
        [Compare("Password",ErrorMessage ="Parollar eyni deyil")]
        public string? ConfirmPassword { get; set; }
    }
}
