using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract
{
    public interface ISubProfessionService
    {
        Task<SubProfession> GetSubProfessionByIdAsync(int id);
        Task<IEnumerable<SubProfession>> GetAllSubProfessionsAsync(int? sectionId);
        Task AddSubProfessionAsync(SubProfession entity);
        Task UpdateSubProfessionAsync(SubProfession entity);
        Task DeleteSubProfessionAsync(int id);
    }
}
