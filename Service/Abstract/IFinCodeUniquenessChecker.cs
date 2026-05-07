namespace ForQab.Service.Abstract
{
    /// <summary>
    /// Yeni əlavə olunan/redaktə edilən rekordlarda FinCode-un unikallığını yoxlayır.
    /// Mövcud (köhnə) rekordlara toxunmur — yalnız yeni daxil edilən və ya redaktə nəticəsində
    /// dəyişdirilən FinCode-u digər rekordların FinCode-u ilə müqayisə edir.
    /// </summary>
    public interface IFinCodeUniquenessChecker
    {
        /// <summary>
        /// Monitors cədvəlində FinCode istifadədədirmi? (excludeId — Edit zamanı redaktə olunan rekordu istisna etmək üçün)
        /// </summary>
        Task<bool> IsMonitorFinCodeTakenAsync(string? finCode, int? excludeId = null);

        /// <summary>
        /// Experts cədvəlində FinCode istifadədədirmi?
        /// </summary>
        Task<bool> IsExpertFinCodeTakenAsync(string? finCode, int? excludeId = null);

        /// <summary>
        /// DimRepresentatives cədvəlində FinCode istifadədədirmi? (Type = 1 → DİM, Type = 2 → Nazirlik)
        /// </summary>
        Task<bool> IsRepresentativeFinCodeTakenAsync(string? finCode, int? type = null, int? excludeId = null);
    }
}
