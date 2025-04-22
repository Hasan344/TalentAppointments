using ForQab.DataAccess.Models;
using ForQab.Migrations;
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository.Abstract
{
    public interface INaturaRepository : IBaseRepository<Monitor>
    {
        public Task<List<Monitor>> GetAllAsync(int? sectionId, int? role, Expression<Func<Monitor, bool>> exp = null, params string[] includes);
        public Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
    }
}
