using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Worker;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Repository.Concrete
{
    public class WorkerRepository : BaseRepository<Monitor>, IWorkerRepository
    {
        private readonly MyDbContext _dbContext;
        public WorkerRepository(MyDbContext dbContext) : base(dbContext)
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
                    query = query.Include(item);  
                }
            }
            return query;
        }

        public async Task<IEnumerable<Monitor>> GetMonitorLogsAsync()
        {
            return await _dbContext.Monitors
                        .Include(ml => ml.MonitorLogs)
                        .Where(ml => ml.Role == 5 && ml.MonitorLogs.Any())
                        .ToListAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _dbContext.Monitors
                        .Include(ml => ml.MonitorLogs)
                        .Where(ml => ml.Id == monitorId)
                        .ToListAsync();
        }

        public async Task UpdateAsync(WorkerEditViewModel model)
        {
            var monitor = await _dbContext.Monitors.FindAsync(model.Id);
            if (monitor == null)
            {
                throw new KeyNotFoundException("Monitor tapılmadı.");
            }

            monitor.Name = model.Name;
            monitor.Surname = model.Surname;
            monitor.Fname = model.Fname;
            monitor.Region = model.Region;
            monitor.SectionId = model.SectionId;
            monitor.WorkerType = model.WorkerType;
            monitor.Gender = model.Gender;
            monitor.BirthDate = model.BirthDate;
            monitor.SSN = model.SSN;
            monitor.Rekvizit = model.Rekvizit;
            monitor.Voen = model.Voen;
            monitor.BankFilial = model.BankFilial;
            monitor.BankFilialCode = model.BankFilialCode;
            monitor.District = model.District;
            monitor.ExamBuildingId = model.ExamBuilding;
            monitor.TelIs = model.TelIs;
            monitor.FinCode = model.FinCode;
            monitor.SerialPrefix = model.SerialPrefix; 
            monitor.Serial = model.Serial;
            monitor.UpdatedAt = DateTime.Now;

            _dbContext.Monitors.Update(monitor);
            await _dbContext.SaveChangesAsync();
        }

    }
}
