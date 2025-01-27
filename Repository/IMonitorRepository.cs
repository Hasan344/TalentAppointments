
using System.Linq.Expressions;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Repository
{
    public interface IMonitorRepository : IBaseRepository<Monitor>
    {
        public Task<List<Monitor>> GetAllAsync(int? sectionId, int? role, Expression<Func<Monitor, bool>> exp = null, params string[] includes);
        public Task BulkAddAsync(IEnumerable<Monitor> monitors);
    }
}
