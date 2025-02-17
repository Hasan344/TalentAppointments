using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using ForQab.Repository;

namespace ForQab.Service
{
    public class KonsService : IKonsService
    {
        private readonly IKonsRepository _konsRepository;

        public KonsService(IKonsRepository konsRepository)
        {
            _konsRepository = konsRepository;
        }

        public async Task AddAsync(KonsViewModel entity)
        {
            await _konsRepository.AddAsync(entity);
        }

        //public async Task BulkAddAsync(IEnumerable<Expert> monitors)
        //{
        //    await _konsRepository.BulkAddAsync(monitors);
        //}

        public async Task DeleteAsync(int id)
        {
            await _konsRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId)
        {
            return await _konsRepository.GetAllAsync(sectionId);
        }
        public Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
        {
            return _konsRepository.GetSubProfessionsAsync(sectionId);
        }
        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
        {
            //var includes = new string[] { "DistrictNavigation", "SubProfessions", "Section", "GenderNavigation" };

            var query = await _konsRepository.GetAllAsync(sectionId);
            if (genderId.HasValue)
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
            if (subProfessionId.HasValue && subProfessionId > 0)
            {
                query = query.Where(m => m.SubProfessions.Any(sp => sp.Id == subProfessionId.Value)).ToList();
            }
            return query;
        }

        public async Task<Expert> GetByIdAsync(int id)
        {
            return await _konsRepository.GetByIdAsync(id);
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            return await _konsRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(KonsEditViewModel entity)
        {
            await _konsRepository.UpdateAsync(entity);
        }
        public async Task BulkAddAsync(IEnumerable<Expert> experts)
        {
            await _konsRepository.BulkAddAsync(experts);
        }
    }
}
