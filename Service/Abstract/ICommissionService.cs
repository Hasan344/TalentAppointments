using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract
{
    public interface ICommissionService
    {
        Task<Commission> GetCommissionByIdAsync(int id);
        Task<IEnumerable<Commission>> GetAllCommissionsAsync(int? sectionId);
        Task AddCommissionAsync(Commission entity);
        Task UpdateCommissionAsync(Commission entity);
        Task DeleteCommissionAsync(int id);
        Task<List<Section>> GetSectionByIdAsync(int? sectionId);
    }
}
