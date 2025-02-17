using ForQab.DataAccess.Models;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service
{
    public interface INaturaService
    {
        Task<Monitor> GetByIdAsync(int id);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear);
        Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear);
        Task AddAsync(Monitor entity);
        Task UpdateAsync(Monitor entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<string> ImportFromExcelAsync(IFormFile excelFile);
        Task<byte[]> ExportToExcelAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync();
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
    }
}
