using FluentValidation;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// MonitorViewModel (Create) üçün FluentValidation qaydaları.
    /// FinCode unikallıq yoxlaması yalnız YENİ daxil olanlar üçün işləyir —
    /// sistemdə artıq mövcud olan dublikatlara müdaxilə etmir.
    /// </summary>
    public class MonitorValidator : AbstractValidator<MonitorViewModel>
    {
        public MonitorValidator(IFinCodeUniquenessChecker uniquenessChecker)
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage("Ad boş ola bilməz");
            RuleFor(m => m.Surname).NotEmpty().WithMessage("Soyad boş ola bilməz");
            RuleFor(m => m.Fname).NotEmpty().WithMessage("Ata adı boş ola bilməz");
            RuleFor(m => m.BirthDate).NotNull().WithMessage("Doğum tarixi boş ola bilməz");

            RuleFor(m => m.FinCode)
                .NotEmpty().WithMessage("FİN kod boş ola bilməz")
                .Length(7).WithMessage("FİN kod 7 simvoldan ibarət olmalıdır")
                .DependentRules(() =>
                {
                    // FinCode unique olmalıdır (yalnız yeni daxil olanlar üçün) —
                    // Create-də Id = 0 olduğu üçün excludeId = null verilir, bütün cədvəl yoxlanılır.
                    RuleFor(m => m.FinCode!)
                        .MustAsync(async (finCode, ct) =>
                            !await uniquenessChecker.IsMonitorFinCodeTakenAsync(finCode))
                        .WithMessage(m => $"'{m.FinCode}' FİN kodu artıq başqa nəzarətçidə qeydiyyatdadır.");
                });

            RuleFor(m => m.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(m => m.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");
            RuleFor(m => m.BankFilial).NotEmpty().WithMessage("Bank filialı boş ola bilməz");
            RuleFor(m => m.BankFilialCode).NotEmpty().WithMessage("Bank filial kodu boş ola bilməz");
            RuleFor(m => m.SSN).NotEmpty().WithMessage("SSN boş ola bilməz");
            RuleFor(m => m.Rekvizit).NotEmpty().WithMessage("Rekvizit boş ola bilməz");
            RuleFor(m => m.District).NotEmpty().WithMessage("Rayon seçilməlidir");
            RuleFor(m => m.SectionId).GreaterThan(0).WithMessage("İstiqamət seçilməlidir");
        }
    }
}
