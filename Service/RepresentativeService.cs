using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service
{
    public class RepresentativeService : IRepresentativeService
    {
        private readonly IRepresentativeRepository _repository;

        public RepresentativeService(IRepresentativeRepository repository)
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
    }
}
