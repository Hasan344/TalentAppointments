
using DocumentFormat.OpenXml.InkML;
using ForQab.DataAccess.Models;
using ForQab.Extensions;
using ForQab.Migrations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly MyDbContext _dbContext;

        public BaseRepository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return await _dbContext.Set<T>()
                .ToListAsync();
            }
            return await _dbContext.Set<T>()
                .Where(e => EF.Property<int>(e, "SectionId") == sectionId)
                .ToListAsync();
        }

        public async Task<List<T>> GetAllAsync(int? sectionId,  Expression<Func<T, bool>> exp = null, params string[] includes)
        {
            IQueryable<T> query = GetQuery(includes);
            return sectionId is null
                ? await query.ToListAsync()
                : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            if (sectionId==null)
            {
                return _dbContext.Sections.ToList();
            }
            return _dbContext.Sections.Where(s=>s.Id==sectionId).ToList();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
        }
        private IQueryable<T> GetQuery(string[] includes)
        {
            IQueryable<T> query = _dbContext.Set<T>();
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
