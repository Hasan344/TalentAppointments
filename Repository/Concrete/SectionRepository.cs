using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository.Concrete
{
    public class SectionRepository : ISectionRepository
    {
        private readonly MyDbContext _context;

        public SectionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Section>> GetAllAsync()
            => await _context.Sections.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<Section>> GetByIdAsync(int sectionId)
            => await _context.Sections.Where(s => s.Id == sectionId).AsNoTracking().ToListAsync();
    }
}
