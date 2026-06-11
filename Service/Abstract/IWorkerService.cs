using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Worker;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service.Abstract
{
    public interface IWorkerService
    {
        Task<Monitor> GetByIdAsync(int id);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, DateTime? createdStartDate = null, DateTime? createdEndDate = null, int ? workerTypeId = null);
        Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task AddAsync(Monitor entity);
        Task UpdateAsync(Monitor entity);
        Task UpdateModelAsync(WorkerEditViewModel entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<string> ImportFromExcelAsync(IFormFile excelFile);
        Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, DateTime? createdStartDate = null, DateTime? createdEndDate = null);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync();
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
        Task<byte[]> ExportContractsToWordAsync(List<int> selectedExpertIds, DateTime contractDate, int workerType);
        //Task<byte[]> ExportContractToWordAsync(int monitorId);
    }
}
