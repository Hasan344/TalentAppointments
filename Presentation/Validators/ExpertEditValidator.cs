using FluentValidation;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// ExpertEditViewModel (Edit) üçün FluentValidation qaydaları.
    /// </summary>
    public class ExpertEditValidator : AbstractValidator<ExpertEditViewModel>
    {
        public ExpertEditValidator(IFinCodeUniquenessChecker uniquenessChecker)
        {
            RuleFor(e => e.Name).NotEmpty().WithMessage("Ad boş ola bilməz");
            RuleFor(e => e.Surname).NotEmpty().WithMessage("Soyad boş ola bilməz");
            RuleFor(e => e.Fname).NotEmpty().WithMessage("Ata adı boş ola bilməz");
            RuleFor(e => e.SectionId).NotNull().GreaterThan(0).WithMessage("İstiqamət seçilməlidir");

            RuleFor(e => e.FinCode)
                .NotEmpty().WithMessage("FİN kod boş ola bilməz")
                .Length(7).WithMessage("FİN kod 7 simvoldan ibarət olmalıdır")
                .DependentRules(() =>
                {
                    RuleFor(e => e)
                        .MustAsync(async (model, ct) =>
                            !await uniquenessChecker.IsExpertFinCodeTakenAsync(model.FinCode, model.Id))
                        .WithMessage(e => $"'{e.FinCode}' FİN kodu artıq başqa ekspertdə qeydiyyatdadır.")
                        .OverridePropertyName(nameof(ExpertEditViewModel.FinCode));
                });

            RuleFor(e => e.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(e => e.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");
        }
    }
}
