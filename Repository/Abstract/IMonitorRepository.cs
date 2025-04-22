using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Migrations;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository.Abstract
{
    public interface IMonitorRepository : IBaseRepository<Monitor>
    {
        public Task<List<Monitor>> GetAllAsync(int? sectionId, int? role, Expression<Func<Monitor, bool>> exp = null, params string[] includes);
        public Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
        Task DeleteMonitorLogs(int? id);
        Task UpdateAsync(MonitorEditViewModel model); 
        Task<List<Monitor>> GetAvailableMonitorsAsync(int sectionId, int role, int gender, List<int> selectedMonitorList);
        Task<List<Monitor>> GetAvailableWorkersAsync(int sectionId, int role, int workerType, List<int> selectedMonitorList);
        Task<int?> GetMonitorAttributeByIdAsync(int monitorId, int role);
        Task<Monitor> GetMonitorByIdAsync(int monitorId); 
        Task<List<Monitor>> GetMonitorsByIdsAsync(List<int> monitorIds);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
    }
}
