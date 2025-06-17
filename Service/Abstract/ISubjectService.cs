using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract
{
    public interface ISubjectService
    {
        Task<Subject> GetSubjectByIdAsync(int id);
        Task<IEnumerable<Subject>> GetAllSubjectsAsync(int? sectionId);
        Task AddSubjectAsync(Subject entity);
        Task UpdateSubjectAsync(Subject entity);
        Task DeleteSubjectAsync(int id);
    }
}
