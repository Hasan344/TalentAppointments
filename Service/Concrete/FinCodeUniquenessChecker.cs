using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Service.Concrete
{
    /// <summary>
    /// FinCode unikallıq yoxlayıcısının konkret tətbiqi.
    /// QEYD: Bu yoxlama YALNIZ yeni daxil olanlar / redaktə zamanı dəyişdirilənlər üçün işləyir.
    /// Sistemdə artıq mövcud olan dublikatlara müdaxilə etmir.
    /// </summary>
    public class FinCodeUniquenessChecker : IFinCodeUniquenessChecker
    {
        private readonly MyDbContext _context;

        public FinCodeUniquenessChecker(MyDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsMonitorFinCodeTakenAsync(string? finCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(finCode))
                return false; // boş FinCode unikallıq yoxlamasından kənardır (NotEmpty validator ayrıca çıxaracaq)

            var normalized = finCode.Trim();

            return await _context.Monitors
                .AsNoTracking()
                .AnyAsync(m => m.FinCode == normalized
                            && (excludeId == null || m.Id != excludeId.Value));
        }

        public async Task<bool> IsExpertFinCodeTakenAsync(string? finCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(finCode))
                return false;

            var normalized = finCode.Trim();

            return await _context.Experts
                .AsNoTracking()
                .AnyAsync(e => e.FinCode == normalized
                            && (excludeId == null || e.Id != excludeId.Value));
        }

        public async Task<bool> IsRepresentativeFinCodeTakenAsync(string? finCode, int? type = null, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(finCode))
                return false;

            var normalized = finCode.Trim();

            var query = _context.DimRepresentatives.AsNoTracking()
                .Where(r => r.FinCode == normalized);

            if (type.HasValue)
                query = query.Where(r => r.Type == type.Value);

            if (excludeId.HasValue)
                query = query.Where(r => r.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
