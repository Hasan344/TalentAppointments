using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete;

public class DistrictRepository : BaseRepository<District>, IDistrictRepository
{
    private readonly MyDbContext _dbContext;

    public DistrictRepository(MyDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<District>> GetAllAsync()
    {
        return _dbContext.Districts.ToListAsync();
    }
}
