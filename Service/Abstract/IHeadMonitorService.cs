using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.DataAccess.ViewModel.Monitor;
using Microsoft.EntityFrameworkCore;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service.Abstract
{
    public interface IHeadMonitorService
    {
        Task<Monitor> GetByIdAsync(int id);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear);
        Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear);
        Task AddAsync(Monitor entity);
        Task UpdateAsync(Monitor entity);
        Task DeleteAsync(int id);
        Task<List<Section>> GetSectionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
        Task<string> ImportFromExcelAsync(IFormFile excelFile);
        Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear);
        Task<IEnumerable<Monitor>> GetMonitorLogsAsync(int? sectionId);
        Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId);
        Task DeleteMonitorLogs(int? id);
        Task UpdateModelAsync(HeadMonitorEditViewModel model);
        Task<byte[]> ExportContractsToWordAsync(List<int> selectedMonitorIds, DateTime contractDate);
    }
}
