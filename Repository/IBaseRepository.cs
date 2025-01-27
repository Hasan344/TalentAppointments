using ForQab.DataAccess.Models;
using ForQab.Migrations;
using System.Linq.Expressions;

namespace ForQab.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(int? sectionId);
        Task<List<T>> GetAllAsync(int? sectionId,  Expression<Func<T, bool>> exp = null, params string[] includes);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<List<Section>> GetSectionsAsync(int? sectionId);
    }
}
