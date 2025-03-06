using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract;

public interface IExamBuildingService
{
    Task<ExamBuilding> GetExamBuildingByIdAsync(int id);
    Task<IEnumerable<ExamBuilding>> GetAllExamBuildingsAsync(int? sectionId);
    Task AddExamBuildingAsync(ExamBuilding entity);
    Task UpdateExamBuildingAsync(ExamBuilding entity);
    Task DeleteExamBuildingAsync(int id);
    Task<IEnumerable<Section>> GetAllSectionsAsync();
    Task<IEnumerable<Section>> GetSectionsByIdAsync(int sectionId);
}