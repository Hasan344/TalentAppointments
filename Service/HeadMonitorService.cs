using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service
{
    public class HeadMonitorService : IHeadMonitorService
    {
        private readonly IHeadMonitorRepository _headMonitorRepository;

        public HeadMonitorService(IHeadMonitorRepository headMonitorRepository)
        {
            _headMonitorRepository = headMonitorRepository;
        }

        public async Task AddAsync(DataAccess.Models.Monitor entity)
        {
            await _headMonitorRepository.AddAsync(entity);
        }

        public async Task BulkAddAsync(IEnumerable<DataAccess.Models.Monitor> monitors)
        {
            await _headMonitorRepository.BulkAddAsync(monitors);
        }

        public async Task DeleteAsync(int id)
        {
            await _headMonitorRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<DataAccess.Models.Monitor>> GetAllAsync(int? sectionId)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _headMonitorRepository.GetAllAsync(sectionId, 2, null, includes);
            return await _headMonitorRepository.GetAllAsync(sectionId, 2, null, includes);
        }
        public async Task<IEnumerable<DataAccess.Models.Monitor>> GetAllAsync(int? sectionId,string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };

            var query = await _headMonitorRepository.GetAllAsync(sectionId, 2, null, includes);
            if (genderId.HasValue && genderId > 0)
            {
                query = query.Where(m => m.Gender == genderId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(m =>
             m.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
             m.Surname.Contains(searchName, StringComparison.OrdinalIgnoreCase))
              .ToList();
            }
            // FinCode filtresi
            if (!string.IsNullOrEmpty(finCode))
            {
                query = query.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Serial filtresi
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // District filtresi
            if (district.HasValue && district > 0)
            {
                query = query.Where(m => m.District == district.Value).ToList();
            }
            if (startYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year >= startYear.Value).ToList(); // Tarixi ilin başlanğıcına çeviririk.
            if (endYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year <= endYear.Value).ToList();

            return query;
        }

        public async Task<DataAccess.Models.Monitor> GetByIdAsync(int id)
        {
            return await _headMonitorRepository.GetByIdAsync(id);
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            return await _headMonitorRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(DataAccess.Models.Monitor entity)
        {
            await _headMonitorRepository.UpdateAsync(entity);
        }
    }
}
