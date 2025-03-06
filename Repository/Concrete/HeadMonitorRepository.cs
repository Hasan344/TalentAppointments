using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository.Concrete
{
    public class HeadMonitorRepository : BaseRepository<Monitor>, IHeadMonitorRepository
    {
        private readonly MyDbContext _dbContext;
        public HeadMonitorRepository(MyDbContext dbContext) : base(dbContext)
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
                        .Include(m => m.MonitorLogs)
                        .Where(m => m.Role == 1 && m.MonitorLogs.Any())
                        .ToListAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _dbContext.Monitors
                        .Include(ml => ml.MonitorLogs)
                        .Where(ml => ml.Id == monitorId)
                        .ToListAsync();
        }

        public async Task DeleteMonitorLogs(int? id)
        {
            var log = await _dbContext.MonitorLogs.FindAsync(id);
            if (log == null)
            {
                throw new InvalidOperationException("Log yoxdur");
            }
            else
                _dbContext.MonitorLogs.Remove(log);
            _dbContext.SaveChanges();
        }
        public async Task UpdateAsync(HeadMonitorEditViewModel model)
        {
            var monitor = await _dbContext.Monitors.FindAsync(model.Id);
            if (monitor == null)
            {
                throw new KeyNotFoundException("Monitor tapılmadı.");
            }

            // Güncellenmesi gereken alanlar
            monitor.Name = model.Name;
            monitor.Surname = model.Surname;
            monitor.Fname = model.Fname;
            monitor.Region = model.Region;
            monitor.SectionId = model.SectionId;
            monitor.Gender = model.Gender;
            monitor.BirthDate = model.BirthDate;
            monitor.SSN = model.SSN;
            monitor.Rekvizit = model.Rekvizit;
            monitor.Voen = model.Voen;
            monitor.BankFilial = model.BankFilial;
            monitor.BankFilialCode = model.BankFilialCode;
            monitor.District = model.District;
            monitor.Uni = model.Uni;
            monitor.ContractNo = model.ContractNo;
            monitor.ContractDate = model.ContractDate;
            monitor.Profession = model.Profession;
            monitor.Position = model.Position;

            _dbContext.Monitors.Update(monitor);
            await _dbContext.SaveChangesAsync();
        }
    }
}