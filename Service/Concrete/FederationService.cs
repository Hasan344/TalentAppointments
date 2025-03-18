using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;

namespace ForQab.Service
{
    public class FederationService : IFederationService
    {
        private readonly IFederationRepository _repository;

        public FederationService(IFederationRepository repository)
        {
            _repository = repository;
        }

        public async Task AddFederationAsync(Profession entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task DeleteFederationAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<Profession> GetFederationByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Profession>> GetAllFederationsAsync(int? sectionId)
        {
            var includes = new string[] { "Section" };
            return await _repository.GetAllAsync(sectionId, null, includes);
        }

        public async Task UpdateFederationAsync(Profession entity)
        {
            await _repository.UpdateAsync(entity);
        }
    }
}
