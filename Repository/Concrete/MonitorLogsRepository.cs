using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;

namespace ForQab.Repository.Concrete
{
    public class MonitorLogsRepository : BaseRepository<MonitorLog>, IMonitorLogsRepository
    {
        private readonly MyDbContext _dbContext;
        public MonitorLogsRepository(MyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
