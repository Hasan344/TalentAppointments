using ForQab.DataAccess.Models;

namespace ForQab.Repository.Abstract
{
    public interface IExamMonitorRepository
    {
        Task<ExamMonitor?> GetByExamAndMonitorAsync(int examId, int monitorId);
        Task AddAsync(ExamMonitor examMonitor);
        void Remove(ExamMonitor examMonitor);
    }
}
