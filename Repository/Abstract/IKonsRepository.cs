using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface IKonsRepository
    {
        public Task<List<Expert>> GetAllAsync(int? sectionId, int? role, Expression<Func<Expert, bool>> exp = null, params string[] includes);
        //public Task BulkAddAsync(IEnumerable<Expert> experts);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
        public Task UpdateAsync(KonsEditViewModel entity);
        public Task UpdateAsync(Expert entity);
        Task<Expert> GetByIdAsync(int id);
        Task<IEnumerable<Expert>> GetAllAsync(int? sectionId);
        Task AddAsync(KonsViewModel entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Expert>> GetKonsLogsAsync(int? sectionId);
        Task<List<Section>> GetSectionsAsync(int? sectionId);
        public Task BulkAddAsync(IEnumerable<Expert> experts);
    }
}
