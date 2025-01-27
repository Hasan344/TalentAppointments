using System.ComponentModel.DataAnnotations;

namespace ForQab.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="İstifadəçi adı yazılmalıdır")]
        [DataType(DataType.Text)]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Parol yazılmalıdır")]
        [DataType(DataType.Password)]
        public string? Password{ get; set; }
    }
}
