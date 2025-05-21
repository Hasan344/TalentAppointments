using DocumentFormat.OpenXml.Office2010.Excel;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using System.Drawing;
using System.Threading;

namespace ForQab.Service;

public class ExpertService : IExpertService
{
    private readonly IExpertRepository _expertRepository;

    public ExpertService(IExpertRepository expertRepository)
    {
        _expertRepository = expertRepository;
    }

    public async Task<IEnumerable<Expert>> GetAllExpertsAsync()
    {
        var experts = await _expertRepository.GetAllAsync();
        if (experts == null)
            throw new Exception("Expert tapılmadı.");
        return experts;
    }

    public async Task<Expert?> GetExpertByIdAsync(int id)
    {
        var expert = await _expertRepository.GetByIdAsync(id);
        if (expert == null)
            throw new Exception($"{id} Id-yə sahib expert yoxdur.");

        string photoPath = $@"\\teshkilat-db\Images\Talent\{expert.FinCode}.jpg";
        expert.Photo = ConvertToBase64(photoPath);

        return expert;
    }
    private string ConvertToBase64(string imagePath, int width = 150, int height = 150)
    {
        if (!System.IO.File.Exists(imagePath))
            return null;

        try
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            using var inputStream = new MemoryStream(imageBytes);
            using var image = Image.FromStream(inputStream);

            using var resized = new Bitmap(image, new Size(width, height));
            using var outputStream = new MemoryStream();
            resized.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            string base64String = Convert.ToBase64String(outputStream.ToArray());
            return $"data:image/jpeg;base64,{base64String}";
        }
        catch
        {
            return null;
        }
    }

    public async Task AddExpertAsync(ExpertViewModel expertViewModel)
    {
        // Biznes qaydaları: Ekspert adının boş olmaması
        if (string.IsNullOrWhiteSpace(expertViewModel.Name))
            throw new ArgumentException("Ad boş ola bilməz.");

        await _expertRepository.AddAsync(expertViewModel);
    }

    public async Task UpdateExpertAsync(ExpertEditViewModel expert)
    {
        // Biznes qaydaları: Ekspert ID-si yoxlanılır
        if (expert.Id <= 0)
            throw new ArgumentException("Id Xətası.");

        var existingExpert = await _expertRepository.GetByIdAsync(expert.Id);
        if (existingExpert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.UpdateAsync(expert);
    }

    public async Task DeleteExpertAsync(int id)
    {
        var expert = await _expertRepository.GetByIdAsync(id);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.DeleteAsync(id);
    }

    public async Task AddSubProfessionToExpertAsync(int expertId, SubProfession subProfession)
    {
        // Biznes qaydaları: SubProfession adı yoxlanılır
        if (string.IsNullOrWhiteSpace(subProfession.Name))
            throw new ArgumentException("SubProfession name cannot be empty.");

        var expert = await _expertRepository.GetByIdAsync(expertId);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.AddSubProfessionToExpertAsync(expertId, subProfession);
    }

    public async Task RemoveSubProfessionFromExpertAsync(int expertId, int subProfessionId)
    {
        var expert = await _expertRepository.GetByIdAsync(expertId);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.RemoveSubProfessionFromExpertAsync(expertId, subProfessionId);
    }

    public async Task<IEnumerable<Expert>> SearchExpertsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Search term cannot be empty.");

        return await _expertRepository.SearchByNameAsync(name);
    }
    public async Task<List<Section>> GetSectionsAsync(int? sectionId)
    {
        return await _expertRepository.GetSectionsAsync(sectionId);  
    }
    public async Task<List<Profession>> GetFederationsAsync(int? sectionId)
    {
        return await _expertRepository.GetFederationsAsync(sectionId);
    }

    public Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
    {
        return _expertRepository.GetSubProfessionsAsync(sectionId);
    }
    public async Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions.SubProfession", "Section", "GenderNavigation", "FederationNavigation", "Contracts" };
        return await _expertRepository.GetAllAsync(sectionId,null,includes);
    }
    public async Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? federationId, int? subProfessionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions.SubProfession", "Section", "GenderNavigation", "FederationNavigation", "Contracts" };

        var query = await _expertRepository.GetAllAsync(sectionId, null, includes); 
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
            query = query.Where(m => m.District.HasValue && m.District == district.Value).ToList();
        }
        if (startYear.HasValue)
            query = query.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year >= startYear.Value).ToList();

        if (endYear.HasValue)
            query = query.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year <= endYear.Value).ToList();

        if (federationId.HasValue)
        {
            query = query.Where(f => f.Federation.HasValue && f.Federation ==  federationId).ToList();
        }

        if (subProfessionId.HasValue && subProfessionId > 0)
        {
            query = query.Where(m => m.ExpertsProfessions.Any(sp => sp.SubProfessionId == subProfessionId.Value)).ToList();
        }

        return query;
    }
    public async Task<IEnumerable<Expert>> GetArchivedExpertsBySectionIdAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions", "Section", "GenderNavigation", "FederationNavigation", "Contracts" };

        var query = await _expertRepository.GetAllArchivedAsync(sectionId, null, includes);
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
            query = query.Where(m => m.ExpertsProfessions.Any(sp => sp.SubProfessionId == subProfessionId.Value)).ToList();
        }

        return query;
    }
    public async Task BulkAddAsync(IEnumerable<Expert> experts)
    {
        await _expertRepository.BulkAddAsync(experts);
    }

    public async Task<IEnumerable<Expert>> GetExpertLogsAsync(int? sectionId)
    {
        return await _expertRepository.GetExpertLogsAsync(sectionId);
    }

    public async Task<IEnumerable<Expert>> GetExpertLogsByExpertIdAsync(int expertId)
    {
        return await _expertRepository.GetExpertLogsByExpertIdAsync(expertId);
    }

    public async Task DeleteExpertLogs(int? id)
    {
        await _expertRepository.DeleteExpertLogs(id);
    }

    public async Task UpdateExpertAsync(Expert expert)
    {
        await _expertRepository.UpdateExpertAsync(expert);
    }
}
