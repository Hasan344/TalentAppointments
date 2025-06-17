using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;

namespace ForQab.Service
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _repository;

        public SubjectService(ISubjectRepository repository)
        {
            _repository = repository;
        }

        public async Task AddSubjectAsync(Subject entity)
        {
            await _repository.AddAsync(entity);
        }

        public async Task DeleteSubjectAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<Subject> GetSubjectByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsAsync(int? sectionId)
        {
            var includes = new string[] { "Section" };
            return await _repository.GetAllAsync(sectionId, null, includes);
        }

        public async Task UpdateSubjectAsync(Subject entity)
        {
            await _repository.UpdateAsync(entity);
        }
    }
}
