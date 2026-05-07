using FluentValidation;
using ForQab.Service.Abstract;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// HeadMonitor (Create) validatoru — Monitor entity-si ilə işləyir.
    /// FinCode unikallığı bütün Monitors cədvəlində yoxlanılır
    /// (HeadMonitor da Monitor cədvəlində Role=1 ilə saxlanılır).
    /// </summary>
    public class HeadMonitorValidator : AbstractValidator<Monitor>
    {
        public HeadMonitorValidator(IFinCodeUniquenessChecker uniquenessChecker)
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
                    RuleFor(m => m)
                        .MustAsync(async (model, ct) =>
                        {
                            int? excludeId = model.Id > 0 ? model.Id : null;
                            return !await uniquenessChecker.IsMonitorFinCodeTakenAsync(model.FinCode, excludeId);
                        })
                        .WithMessage(m => $"'{m.FinCode}' FİN kodu artıq başqa şəxsdə qeydiyyatdadır.")
                        .OverridePropertyName(nameof(Monitor.FinCode));
                });

            RuleFor(m => m.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(m => m.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");
            RuleFor(m => m.BankFilial).NotEmpty().WithMessage("Bank filialı boş ola bilməz");
            RuleFor(m => m.BankFilialCode).NotEmpty().WithMessage("Bank filial kodu boş ola bilməz");
            RuleFor(m => m.SSN).NotEmpty().WithMessage("SSN boş ola bilməz");
            RuleFor(m => m.Rekvizit).NotEmpty().WithMessage("Rekvizit boş ola bilməz");
            RuleFor(m => m.District).NotEmpty().WithMessage("Rayon seçilməlidir");
            RuleFor(m => m.SectionId).NotNull().GreaterThan(0).WithMessage("İstiqamət seçilməlidir");
        }
    }
}
