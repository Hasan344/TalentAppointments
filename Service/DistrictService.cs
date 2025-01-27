using ForQab.DataAccess.Models;
using ForQab.Repository;
using ForQab.Service;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _repository;

    public DistrictService(IDistrictRepository repository)
    {
        _repository = repository;
    }

    public async Task AddDistrictAsync(District entity)
    {
        await _repository.AddAsync(entity);
    }

    public async Task DeleteDistrictAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<District> GetDistrictByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<District>> GetAllDistrictsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task UpdateDistrictAsync(District entity)
    {
        await _repository.UpdateAsync(entity);
    }
}