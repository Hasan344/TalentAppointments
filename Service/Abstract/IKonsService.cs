using ForQab.Data_Access.ViewModel.Expert;
using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;

namespace ForQab.Service.Abstract
{
    public interface IKonsService
    {
        Task<Expert> GetByIdAsync(int id);
        Task<IEnumerable<Expert>> GetAllAsync(int? sectionId);
        Task<IEnumerable<Expert>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId);
        Task AddAsync(KonsViewModel entity);
        Task UpdateAsync(KonsEditViewModel entity);
        Task UpdateAsync(Expert entity);
        Task DeleteAsync(int id);
        Task<List<Section>> GetSectionsAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Expert> experts);
    }
}
