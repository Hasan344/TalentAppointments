using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.Data_Access.ViewModel
{
    public class ExpertViewModel
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Fname { get; set; }
        public int? SectionId { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? SSN { get; set; }
        public string? Rekvizit { get; set; }
        public string? HesablashmaH { get; set; }
        public string? Voen { get; set; }
        public string? BankFilial { get; set; }
        public string? BankFilialCode { get; set; }
        public Boolean? Kons { get; set; }
        public string? FinCode { get; set; }
        public string? Profession { get; set; }
        public int? Federation { get; set; }
        public byte? Gender { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; } // For dropdown list
        public int[]? SelectedSubProfessions { get; set; } // IDs of selected SubProfessions
    }

    public class SubProfessionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
