using ForQab.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository
{
    public class CommissionRepository : BaseRepository<Commission>, ICommissionRepository
    {
        private readonly MyDbContext _dbContext;
        public CommissionRepository(MyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Commission>> GetAllAsync(int? sectionId, int? role, Expression<Func<Commission, bool>> exp = null, params string[] includes)
        {
            IQueryable<Commission> query = GetQuery(includes);
            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }
        private IQueryable<Commission> GetQuery(string[] includes)
        {
            IQueryable<Commission> query = _dbContext.Set<Commission>();
            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);  // Ensure the paths are valid
                }
            }
            return query;
        }
    }
}