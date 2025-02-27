using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.DataAccess.ViewModel.Monitor;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository
{
    public interface IHeadMonitorRepository : IBaseRepository<Monitor>
    {
        Task<List<Monitor>> GetAllAsync(int? sectionId, int? role, Expression<Func<Monitor, bool>> exp = null, params string[] includes);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync();
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
        Task DeleteMonitorLogs(int? id);
        Task UpdateAsync(HeadMonitorEditViewModel model);
    }
}
