using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;

namespace ForQab.Service
{
    public class MinistryRepresentativeService : IMinistryRepresentativeService
    {
        private readonly IMinistryRepresentativeRepository _repository;

        public MinistryRepresentativeService(IMinistryRepresentativeRepository repository)
        {
            _repository = repository;
        }

        public async Task AddRepresentativeAsync(DimRepresentative entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task DeleteRepresentativeAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<DimRepresentative>> GetAllRepresentativesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<DimRepresentative> GetRepresentativeByIdAsync(int id)
        {
             return await _repository.GetByIdAsync(id);
        }

        public async Task UpdateRepresentativeAsync(DimRepresentative entity)
        {
            await _repository.UpdateAsync(entity);
        }
        public async Task<IEnumerable<DimRepresentative>> GetAllArchivedAsync()
        {
            return await _repository.GetAllArchivedAsync();
        }

        public async Task BulkArchiveAsync(List<int> ids, string archiveReason)
        {
            await _repository.BulkArchiveAsync(ids, archiveReason);
        }
    }
}
