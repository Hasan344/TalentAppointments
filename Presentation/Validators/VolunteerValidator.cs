using FluentValidation;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Presentation.Validators
{
    public class VolunteerValidator : AbstractValidator<Monitor>
    {
        public VolunteerValidator()
        {
            RuleFor(m => m.Name).NotEmpty();
            RuleFor(m => m.Surname).NotEmpty();
            RuleFor(m => m.Fname).NotEmpty();
            RuleFor(m => m.ExamBuildingId).NotEmpty();
        }
    }
}
