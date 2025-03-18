using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class FederationRepository : BaseRepository<Profession>, IFederationRepository
    {
        private readonly MyDbContext _context;

        public FederationRepository(MyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Profession>> GetAllAsync(int? sectionId, Expression<Func<Profession, bool>> exp = null, params string[] includes)
        {
            IQueryable<Profession> query = GetQuery(includes);

            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }

        private IQueryable<Profession> GetQuery(string[] includes)
        {
            IQueryable<Profession> query = _context.Set<Profession>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }
    }
}
