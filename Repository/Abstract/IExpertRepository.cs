using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface IExpertRepository
    {
        Task<IEnumerable<Expert>> GetAllAsync();
        Task<Expert?> GetByIdAsync(int id);
        Task AddAsync(ExpertViewModel expertViewModel);
        Task UpdateAsync(ExpertEditViewModel expert);
        Task UpdateExpertAsync(Expert expert);
        Task DeleteAsync(int id);
        //Task<IEnumerable<SubProfession>> GetSubProfessionsByExpertIdAsync(int expertId);
        Task AddSubProfessionToExpertAsync(int expertId, SubProfession subProfession);
        Task RemoveSubProfessionFromExpertAsync(int expertId, int subProfessionId);
        Task<IEnumerable<Expert>> SearchByNameAsync(string name);
        Task<List<Section>> GetSectionsAsync(int? sectionId); 
        Task<List<Expert>> GetExpertsBySectionAndSubProfessionAsync(int sectionId, int subProfessionId, List<int> excludedExpertIds);
        Task<List<Profession>> GetFederationsAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
        Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId);
        Task<List<Expert>> GetAllAsync(int? sectionId, Expression<Func<Expert, bool>> exp = null, params string[] includes);
        Task<List<Expert>> GetAllArchivedAsync(int? sectionId, Expression<Func<Expert, bool>> exp = null, params string[] includes);
        Task BulkAddAsync(IEnumerable<Expert> experts);
        Task<IEnumerable<Expert>> GetExpertLogsAsync();
        Task<IEnumerable<Expert>> GetExpertLogsByExpertIdAsync(int expertId);
        Task DeleteExpertLogs(int? id);
    }
}
