using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;

namespace ForQab.Service
{
    public interface IExpertService
    {
        Task<IEnumerable<Expert>> GetAllExpertsAsync();
        Task<Expert?> GetExpertByIdAsync(int id);
        Task AddExpertAsync(ExpertViewModel expertViewModel);
        Task UpdateExpertAsync(ExpertEditViewModel expert);
        Task DeleteExpertAsync(int id);
        //Task<IEnumerable<SubProfession>> GetSubProfessionsByExpertIdAsync(int expertId);
        Task AddSubProfessionToExpertAsync(int expertId, SubProfession subProfession);
        Task RemoveSubProfessionFromExpertAsync(int expertId, int subProfessionId);
        Task<IEnumerable<Expert>> SearchExpertsByNameAsync(string name);
        Task<List<Section>> GetSectionsAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId);
        Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId);
        Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId);
        public Task BulkAddAsync(IEnumerable<Expert> experts);
    }
}
