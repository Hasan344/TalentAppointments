using ForQab.DataAccess.Models;

namespace ForQab.Repository
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
