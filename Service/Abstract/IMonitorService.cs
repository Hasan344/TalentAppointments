using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Monitor;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service.Abstract
{
    public interface IMonitorService
    {
        Task<Monitor> GetByIdAsync(int id);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear,
    DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, 
    DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task AddAsync(MonitorViewModel entity);
        Task UpdateAsync(MonitorEditViewModel entity);
        Task UpdateAsync(Monitor entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<string> ImportFromExcelAsync(IFormFile excelFile);
        Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
        Task DeleteMonitorLogs(int? id);
        Task UpdateModelAsync(MonitorEditViewModel model);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
        Task<MonitorEditViewModel> GetMonitorForEditAsync(int id);
        Task<byte[]> ExportContractsToWordAsync(List<int> selectedExpertIds, DateTime contractDate);
        Task<List<int>> FilterSelectedMonitorsAsync(
    List<int> selectedIds, string searchName, int? districtId);
        Task<byte[]> ExportContractToWordAsync(int monitorId);
        Task BulkArchiveAsync(List<int> ids, string archiveReason);
    }
}
