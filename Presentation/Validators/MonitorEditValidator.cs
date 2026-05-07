using FluentValidation;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// MonitorEditViewModel (Edit) üçün FluentValidation qaydaları.
    /// FinCode unikallıq yoxlaması cari rekordu istisna edir — yəni eyni rekordun öz FinCode-u
    /// "dublikat" sayılmır. Yalnız BAŞQA monitor-larda eyni FinCode varsa xəta verir.
    /// </summary>
    public class MonitorEditValidator : AbstractValidator<MonitorEditViewModel>
    {
        public MonitorEditValidator(IFinCodeUniquenessChecker uniquenessChecker)
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage("Ad boş ola bilməz");
            RuleFor(m => m.Surname).NotEmpty().WithMessage("Soyad boş ola bilməz");
            RuleFor(m => m.Fname).NotEmpty().WithMessage("Ata adı boş ola bilməz");

            RuleFor(m => m.FinCode)
                .NotEmpty().WithMessage("FİN kod boş ola bilməz")
                .Length(7).WithMessage("FİN kod 7 simvoldan ibarət olmalıdır")
                .DependentRules(() =>
                {
                    RuleFor(m => m)
                        .MustAsync(async (model, ct) =>
                            !await uniquenessChecker.IsMonitorFinCodeTakenAsync(model.FinCode, model.Id))
                        .WithMessage(m => $"'{m.FinCode}' FİN kodu artıq başqa nəzarətçidə qeydiyyatdadır.")
                        .OverridePropertyName(nameof(MonitorEditViewModel.FinCode));
                });

            RuleFor(m => m.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(m => m.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");
            RuleFor(m => m.District).NotEmpty().WithMessage("Rayon seçilməlidir");
            RuleFor(m => m.SectionId).GreaterThan(0).WithMessage("İstiqamət seçilməlidir");
        }
    }
}
