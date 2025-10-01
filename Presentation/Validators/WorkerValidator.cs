using FluentValidation;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Presentation.Validators
{
    public class WorkerValidator : AbstractValidator<Monitor>
    {
        public WorkerValidator() {
            RuleFor(m => m.Name).NotEmpty();
            RuleFor(m => m.Surname).NotEmpty();
            RuleFor(m => m.Fname).NotEmpty();
            RuleFor(m => m.BirthDate).NotNull();
            RuleFor(m => m.FinCode).NotEmpty();
            RuleFor(m => m.SerialPrefix).NotEmpty();
            RuleFor(m => m.Serial).NotEmpty();
            RuleFor(m => m.BankFilial).NotEmpty();
            RuleFor(m => m.BankFilialCode).NotEmpty();
            RuleFor(m => m.SSN).NotEmpty();
            RuleFor(m => m.Rekvizit).NotEmpty();
            RuleFor(m => m.District).NotEmpty();
            RuleFor(m => m.WorkerType).NotEmpty();
            RuleFor(m => m.ExamBuildingId).NotEmpty();
        }
    }
}
