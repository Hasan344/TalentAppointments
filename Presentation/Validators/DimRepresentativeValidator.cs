using FluentValidation;
using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Validators
{
    /// <summary>
    /// DimRepresentative üçün FluentValidation qaydaları.
    /// Həm DİM (Type=1) həm də Nazirlik (Type=2) nümayəndələri üçün istifadə olunur.
    /// Edit zamanı cari rekord öz id-si ilə istisna edilir.
    /// </summary>
    public class DimRepresentativeValidator : AbstractValidator<DimRepresentative>
    {
        public DimRepresentativeValidator(IFinCodeUniquenessChecker uniquenessChecker)
        {
            RuleFor(r => r.Name).NotEmpty().WithMessage("Ad boş ola bilməz");
            RuleFor(r => r.Surname).NotEmpty().WithMessage("Soyad boş ola bilməz");
            RuleFor(r => r.Fname).NotEmpty().WithMessage("Ata adı boş ola bilməz");
            RuleFor(r => r.Tel).NotEmpty().WithMessage("Telefon nömrəsi boş ola bilməz");
            RuleFor(r => r.Serial).NotEmpty().WithMessage("Seriya nömrəsi boş ola bilməz");

            RuleFor(r => r.FinCode)
                .NotEmpty().WithMessage("FİN kod boş ola bilməz")
                .Length(7).WithMessage("FİN kod 7 simvoldan ibarət olmalıdır")
                .DependentRules(() =>
                {
                    RuleFor(r => r)
                        .MustAsync(async (model, ct) =>
                        {
                            // Edit-də Id > 0 olur, bu rekord istisna edilir.
                            // Create-də Id = 0 olur, bu zaman heç nə istisna edilmir (bütün cədvəl yoxlanılır).
                            int? excludeId = model.Id > 0 ? model.Id : null;

                            // Type-spesifik yoxlama: DİM (1) və Nazirlik (2) ayrı-ayrı yoxlanılır.
                            // Eyni şəxs həm DİM, həm Nazirlik nümayəndəsi ola bilər.
                            return !await uniquenessChecker.IsRepresentativeFinCodeTakenAsync(
                                model.FinCode, model.Type, excludeId);
                        })
                        .WithMessage(r => $"'{r.FinCode}' FİN kodu artıq başqa nümayəndədə qeydiyyatdadır.")
                        .OverridePropertyName(nameof(DimRepresentative.FinCode));
                });
        }
    }
}
