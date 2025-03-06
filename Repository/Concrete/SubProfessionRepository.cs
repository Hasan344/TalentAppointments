using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class SubProfessionRepository : BaseRepository<SubProfession>, ISubProfessionRepository
    {
        private readonly MyDbContext _dbContext;

        public SubProfessionRepository(MyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<SubProfession>> GetAllAsync(int? sectionId, Expression<Func<SubProfession, bool>> exp = null, params string[] includes)
        {
            IQueryable<SubProfession> query = GetQuery(includes);

            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }

        private IQueryable<SubProfession> GetQuery(string[] includes)
        {
            IQueryable<SubProfession> query = _dbContext.Set<SubProfession>();
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
