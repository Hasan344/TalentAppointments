
using DocumentFormat.OpenXml.InkML;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository.Concrete
{
    public class NaturaRepository : BaseRepository<Monitor>, INaturaRepository
    {
        private readonly MyDbContext _dbContext;
        public NaturaRepository(MyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task BulkAddAsync(IEnumerable<Monitor> monitors)
        {
            await _dbContext.Set<Monitor>().AddRangeAsync(monitors);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<List<Monitor>> GetAllAsync(int? sectionId, int? role, Expression<Func<Monitor, bool>> exp = null, params string[] includes)
        {
            IQueryable<Monitor> query = GetQuery(includes);
            return sectionId is null
                ? await query.Where(e => EF.Property<int>(e, "Role") == role).ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).Where(e => EF.Property<int>(e, "Role") == role).ToListAsync();
        }
        private IQueryable<Monitor> GetQuery(string[] includes)
        {
            IQueryable<Monitor> query = _dbContext.Set<Monitor>();
            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);  // Ensure the paths are valid
                }
            }
            return query;
        }

        public async Task<IEnumerable<Monitor>> GetMonitorLogsAsync()
        {
            return await _dbContext.Monitors
                        .Include(ml => ml.MonitorLogs)
                        .Where(ml => ml.Role == 3 && ml.MonitorLogs.Any())
                        .ToListAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _dbContext.Monitors
                        .Include(ml => ml.MonitorLogs)
                        .Where(ml => ml.Id == monitorId)
                        .ToListAsync();
        }
    }
}
