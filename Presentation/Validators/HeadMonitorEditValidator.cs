using FluentValidation;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// HeadMonitorEditViewModel (Edit) üçün validator.
    /// FinCode yoxlanışı zamanı cari rekord öz Id-si ilə istisna edilir.
    /// </summary>
    public class HeadMonitorEditValidator : AbstractValidator<HeadMonitorEditViewModel>
    {
        public HeadMonitorEditValidator(IFinCodeUniquenessChecker uniquenessChecker)
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
                        .WithMessage(m => $"'{m.FinCode}' FİN kodu artıq başqa şəxsdə qeydiyyatdadır.")
                        .OverridePropertyName(nameof(HeadMonitorEditViewModel.FinCode));
                });

            RuleFor(m => m.SerialPrefix).NotEmpty().WithMessage("Seriya prefiksi boş ola bilməz");
            RuleFor(m => m.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");
            RuleFor(m => m.District).NotEmpty().WithMessage("Rayon seçilməlidir");
            RuleFor(m => m.SectionId).GreaterThan(0).WithMessage("İstiqamət seçilməlidir");
        }
    }
}
