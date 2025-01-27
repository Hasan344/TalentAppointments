using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IVolunteerRepository _volunteerRepository;

        public VolunteerService(IVolunteerRepository volunteerRepository)
        {
            _volunteerRepository = volunteerRepository;
        }

        public async Task AddAsync(DataAccess.Models.Monitor entity)
        {
            await _volunteerRepository.AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _volunteerRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<DataAccess.Models.Monitor>> GetAllAsync(int? sectionId)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };

            return await _volunteerRepository.GetAllAsync(sectionId, 4, null, includes);
        }

        public async Task<DataAccess.Models.Monitor> GetByIdAsync(int id)
        {
            return await _volunteerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId)
        {
            return await _volunteerRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(DataAccess.Models.Monitor entity)
        {
            await _volunteerRepository.UpdateAsync(entity);
        }
        public async Task BulkAddAsync(IEnumerable<DataAccess.Models.Monitor> monitors)
        {
            await _volunteerRepository.BulkAddAsync(monitors);
        }
    }
}
