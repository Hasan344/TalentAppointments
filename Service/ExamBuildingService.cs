using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service;

public class ExamBuildingService : IExamBuildingService
{
    private readonly IExamBuildingRepository _repository;

    public ExamBuildingService(IExamBuildingRepository repository)
    {
        _repository = repository;
    }

    public async Task AddExamBuildingAsync(ExamBuilding entity)
    {
        await _repository.AddAsync(entity);
    }

    public async Task DeleteExamBuildingAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ExamBuilding>> GetAllExamBuildingsAsync(int? sectionId)
    {
        var includes = new string[] { "Section" };
        return await _repository.GetAllAsync(sectionId, null, includes);
    }

    public async Task<ExamBuilding> GetExamBuildingByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task UpdateExamBuildingAsync(ExamBuilding entity)
    {
        await _repository.UpdateAsync(entity);
    }
}