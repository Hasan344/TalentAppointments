using ForQab.DataAccess.Models;

namespace ForQab.Service;

public interface IExamBuildingService
{
    Task<ExamBuilding> GetExamBuildingByIdAsync(int id);
    Task<IEnumerable<ExamBuilding>> GetAllExamBuildingsAsync(int? sectionId);
    Task AddExamBuildingAsync(ExamBuilding entity);
    Task UpdateExamBuildingAsync(ExamBuilding entity);
    Task DeleteExamBuildingAsync(int id);
}