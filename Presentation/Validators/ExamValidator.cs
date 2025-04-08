using FluentValidation;
using ForQab.DataAccess.Models;

namespace ForQab.Presentation.Validators
{
    public class ExamValidator : AbstractValidator<Exam>
    {
        public ExamValidator() {
            RuleFor(m => m.Name).NotEmpty();
            RuleFor(m => m.EndTime).NotNull();
            RuleFor(m => m.StartTime).NotNull();
            RuleFor(m => m.AdmissionTime).NotNull();
            RuleFor(m => m.SectionId).NotNull();
            RuleFor(m => m.District).NotNull();
            RuleFor(m => m.Shift).NotNull();
            RuleFor(m => m.StudentCount).NotNull();
        }
    }
}
