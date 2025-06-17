using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class SubjectRepository : BaseRepository<Subject>, ISubjectRepository
    {
        private readonly MyDbContext _dbContext;

        public SubjectRepository(MyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Subject>> GetAllAsync(int? sectionId, Expression<Func<Subject, bool>> exp = null, params string[] includes)
        {
            IQueryable<Subject> query = GetQuery(includes);

            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }

        private IQueryable<Subject> GetQuery(string[] includes)
        {
            IQueryable<Subject> query = _dbContext.Set<Subject>();
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
