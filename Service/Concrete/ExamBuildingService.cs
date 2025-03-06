using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;

namespace ForQab.Service;

public class ExamBuildingService : IExamBuildingService
{
    private readonly IExamBuildingRepository _examBuildingRepository;
    private readonly ISectionRepository _sectionRepository;

    public ExamBuildingService(IExamBuildingRepository examBuildingRepository, ISectionRepository sectionRepository)
    {
        _examBuildingRepository = examBuildingRepository;
        _sectionRepository = sectionRepository;
    }

    public async Task AddExamBuildingAsync(ExamBuilding entity)
    {
        await _examBuildingRepository.AddAsync(entity);
    }

    public async Task DeleteExamBuildingAsync(int id)
    {
        await _examBuildingRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ExamBuilding>> GetAllExamBuildingsAsync(int? sectionId)
    {
        var includes = new string[] { "Section" };
        return await _examBuildingRepository.GetAllAsync(sectionId, null, includes);
    }

    public async Task<ExamBuilding> GetExamBuildingByIdAsync(int id)
    {
        return await _examBuildingRepository.GetByIdAsync(id);
    }

    public async Task UpdateExamBuildingAsync(ExamBuilding entity)
    {
        await _examBuildingRepository.UpdateAsync(entity);
    }
    public async Task<IEnumerable<Section>> GetAllSectionsAsync()
        => await _sectionRepository.GetAllAsync();

    public async Task<IEnumerable<Section>> GetSectionsByIdAsync(int sectionId)
        => await _sectionRepository.GetByIdAsync(sectionId);
}