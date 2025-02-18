
using ForQab.Repository;

namespace ForQab.Service
{
    public class MonitorLogsService : IMonitorLogsService
    {
        private readonly IMonitorLogsRepository _repository;

        public MonitorLogsService(IMonitorLogsRepository repository)
        {
            _repository = repository;
        }

        public async Task DeleteDistrictAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
