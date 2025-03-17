using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository.Concrete
{
    public class ExamMonitorRepository : IExamMonitorRepository
    {
        private readonly MyDbContext _context;

        public ExamMonitorRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<ExamMonitor?> GetByExamAndMonitorAsync(int examId, int monitorId)
        {
            return await _context.ExamMonitors
                .FirstOrDefaultAsync(em => em.ExamId == examId && em.MonitorId == monitorId);
        }

        public async Task AddAsync(ExamMonitor examMonitor)
        {
            await _context.ExamMonitors.AddAsync(examMonitor);
        }

        public void Remove(ExamMonitor examMonitor)
        {
            _context.ExamMonitors.Remove(examMonitor);
        }
    }

}
