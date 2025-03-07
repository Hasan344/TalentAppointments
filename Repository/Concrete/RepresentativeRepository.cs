using ForQab.DataAccess.Models;
using ForQab.Migrations;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository.Concrete
{
    public class RepresentativeRepository : BaseRepository<DimRepresentative>, IRepresentativeRepository
    {
        private readonly MyDbContext _context;
        public RepresentativeRepository(MyDbContext dbContext, MyDbContext context) : base(dbContext)
        {
            _context = context;
        }

        public async Task<IEnumerable<DimRepresentative>> GetAllAsync()
        {
            return await _context.DimRepresentatives
                .ToListAsync();
        }
        public async Task<List<DimRepresentative>> GetAvailableRepresentativeAsync(List<int> selectedRepresentativeList)
        {
            return await _context.DimRepresentatives
                .Where(dr => !selectedRepresentativeList.Contains(dr.Id))
                .ToListAsync();
        }
    }
}