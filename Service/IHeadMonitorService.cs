using ForQab.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service
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
        public Task BulkAddAsync(IEnumerable<Monitor> monitors);
        public Task<string> ImportFromExcelAsync(IFormFile excelFile);
        public Task<byte[]> ExportToExcelAsync(int? sectionId);
    }
}
