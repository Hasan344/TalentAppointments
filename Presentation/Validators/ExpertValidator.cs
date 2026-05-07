using FluentValidation;
using ForQab.Data_Access.ViewModel;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// ExpertViewModel (Create) üçün FluentValidation qaydaları.
    /// </summary>
    public class ExpertValidator : AbstractValidator<ExpertViewModel>
    {
        public ExpertValidator(IFinCodeUniquenessChecker uniquenessChecker)
        {
            RuleFor(e => e.Name).NotEmpty().WithMessage("Ad boş ola bilməz");
            RuleFor(e => e.Surname).NotEmpty().WithMessage("Soyad boş ola bilməz");
            RuleFor(e => e.Fname).NotEmpty().WithMessage("Ata adı boş ola bilməz");
            RuleFor(e => e.SectionId).NotNull().GreaterThan(0).WithMessage("İstiqamət seçilməlidir");
            RuleFor(e => e.BirthDate).NotNull().WithMessage("Doğum tarixi boş ola bilməz");
            RuleFor(e => e.SSN).NotEmpty().WithMessage("SSN boş ola bilməz");
            RuleFor(e => e.Rekvizit).NotEmpty().WithMessage("Rekvizit boş ola bilməz");
            RuleFor(e => e.BankFilial).NotEmpty().WithMessage("Bank filialı boş ola bilməz");
            RuleFor(e => e.BankFilialCode).NotEmpty().WithMessage("Bank filial kodu boş ola bilməz");
            RuleFor(e => e.Federation).NotNull().WithMessage("Federasiya seçilməlidir");
            RuleFor(e => e.Gender).NotNull().WithMessage("Cins seçilməlidir");
            RuleFor(e => e.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(e => e.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");

            RuleFor(e => e.FinCode)
                .NotEmpty().WithMessage("FİN kod boş ola bilməz")
                .Length(7).WithMessage("FİN kod 7 simvoldan ibarət olmalıdır")
                .DependentRules(() =>
                {
                    RuleFor(e => e.FinCode!)
                        .MustAsync(async (finCode, ct) =>
                            !await uniquenessChecker.IsExpertFinCodeTakenAsync(finCode))
                        .WithMessage(e => $"'{e.FinCode}' FİN kodu artıq başqa ekspertdə qeydiyyatdadır.");
                });
        }
    }
}
