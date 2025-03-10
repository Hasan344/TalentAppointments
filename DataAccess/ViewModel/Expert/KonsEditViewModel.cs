using Microsoft.AspNetCore.Mvc.Rendering;

namespace ForQab.DataAccess.ViewModel.Expert
{
    public class KonsEditViewModel
    {
        public int Id { get; set; }
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
        public string? TelIs { get; set; }
        public string? TelEl { get; set; }
        public List<SelectListItem>? SubProfessions { get; set; } // For dropdown list
        public int[]? SelectedSubProfessions { get; set; } // IDs of selected SubProfessions
    }
}
