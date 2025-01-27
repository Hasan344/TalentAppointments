using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service
{
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _repository;

        public CommissionService(ICommissionRepository repository)
        {
            _repository = repository;
        }

        public async Task AddCommissionAsync(Commission entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task DeleteCommissionAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        //public async Task<IEnumerable<Commission>> GetAllCommissionsAsync(int? sectionId)
        //{
        //    return await _repository.GetAllAsync(sectionId);
        //}

        public async Task<Commission> GetCommissionByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Section>> GetSectionByIdAsync(int? sectionId)
        {
            return await _repository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateCommissionAsync(Commission entity)
        {
            await _repository.UpdateAsync(entity);
        }
        public async Task<IEnumerable<Commission>> GetAllCommissionsAsync(int? sectionId)
        {
            var includes = new string[] { "Section" };

            return await _repository.GetAllAsync(sectionId, 1, null, includes);
        }
    }
}
