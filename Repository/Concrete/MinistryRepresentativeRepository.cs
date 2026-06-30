using ForQab.DataAccess.Models;
using ForQab.Migrations;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository.Concrete
{
    public class MinistryRepresentativeRepository : BaseRepository<DimRepresentative>, IMinistryRepresentativeRepository
    {
        private readonly MyDbContext _context;
        public MinistryRepresentativeRepository(MyDbContext dbContext, MyDbContext context) : base(dbContext)
        {
            _context = context;
        }

        public async Task<IEnumerable<DimRepresentative>> GetAllAsync()
        {
            return await _context.DimRepresentatives.Where(dr => dr.Type == 2)
                .ToListAsync();
        }
        public async Task<List<DimRepresentative>> GetAvailableRepresentativeAsync(List<int> selectedRepresentativeList)
        {
            return await _context.DimRepresentatives
                .Where(dr => !selectedRepresentativeList.Contains(dr.Id))
                .Where(dr => dr.Type == 2)
                .ToListAsync();
        }
        public async Task<IEnumerable<DimRepresentative>> GetAllArchivedAsync()
        {
            return await _context.DimRepresentatives
                .Where(dr => dr.Type == 2 && dr.Archive == 1)
                .ToListAsync();
        }

        public async Task BulkArchiveAsync(List<int> ids, string archiveReason)
        {
            var representatives = await _context.DimRepresentatives
                .Where(dr => ids.Contains(dr.Id) && dr.Type == 2)
                .ToListAsync();

            foreach (var rep in representatives)
            {
                rep.Archive = 1;
                rep.ArchiveReason = archiveReason;
            }

            await _context.SaveChangesAsync();
        }
    }
}