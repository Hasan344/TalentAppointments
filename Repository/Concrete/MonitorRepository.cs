
using DocumentFormat.OpenXml.InkML;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.DataAccess.ViewModel.Worker;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository.Concrete
{
    public class MonitorRepository : BaseRepository<Monitor>, IMonitorRepository
    {
        private readonly MyDbContext _dbContext;
        public MonitorRepository(MyDbContext dbContext) : base(dbContext)
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
                        .Where(ml => ml.Role == 2 && ml.MonitorLogs.Any())
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
        public async Task UpdateAsync(MonitorEditViewModel model)
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
        public async Task<List<Monitor>> GetAvailableMonitorsAsync(int sectionId, int role, int gender, List<int> selectedMonitorList)
        {
            return await _dbContext.Monitors
                .Where(m => m.Role == role && m.SectionId == sectionId && m.Gender == gender && !selectedMonitorList.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<List<Monitor>> GetAvailableWorkersAsync(int sectionId, int role, int workerType, List<int> selectedMonitorList)
        {
            return await _dbContext.Monitors
                .Where(m => m.Role == role && m.SectionId == sectionId && m.WorkerType == workerType && !selectedMonitorList.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<List<DimRepresentative>> GetAvailableRepresentativesAsync (List<int> selectedRepresentativeList)
        {
            return await _dbContext.DimRepresentatives
                .Where(m => !selectedRepresentativeList.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<int?> GetMonitorAttributeByIdAsync(int monitorId, int role)
        {
            return await _dbContext.Monitors
                .Where(m => m.Id == monitorId && m.Role == role)
                .Select(m => m.WorkerType != null ? m.WorkerType: m.Gender)
                .FirstOrDefaultAsync();
        }

        public async Task<Monitor> GetMonitorByIdAsync(int monitorId)
        {
            try
            {
                return await _dbContext.Monitors
                                       .Where(m => m.Id == monitorId)
                                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error occurred while retrieving monitor with Id {monitorId}: {ex.Message}");
                throw;
            }
        }
        public async Task<List<Monitor>> GetMonitorsByIdsAsync(List<int> monitorIds)
        {
            return await _dbContext.Monitors
                .Where(m => monitorIds.Contains(m.Id))
                .ToListAsync();
        }
    }
}
