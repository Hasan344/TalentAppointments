using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service
{
    public class SubProfessionService : ISubProfessionService
    {
        private readonly ISubProfessionRepository _repository;

        public SubProfessionService(ISubProfessionRepository repository)
        {
            _repository = repository;
        }

        public async Task AddSubProfessionAsync(SubProfession entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task DeleteSubProfessionAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<SubProfession> GetSubProfessionByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<SubProfession>> GetAllSubProfessionsAsync(int? sectionId)
        {
            var includes = new string[] { "Section", "Profession" };
            return await _repository.GetAllAsync(sectionId, null, includes);
        }

        public async Task UpdateSubProfessionAsync(SubProfession entity)
        {
            await _repository.UpdateAsync(entity);
        }
    }
}
