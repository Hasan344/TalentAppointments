using DocumentFormat.OpenXml.InkML;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete;

public class ExamBuildingRepository : BaseRepository<ExamBuilding>, IExamBuildingRepository
{
    private readonly MyDbContext _dbContext;
    public ExamBuildingRepository(MyDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ExamBuilding>> GetAllAsync(int? sectionId, Expression<Func<ExamBuilding, bool>> exp = null, params string[] includes)
    {
        IQueryable<ExamBuilding> query = GetQuery(includes);
        return sectionId == null
            ? await query.ToListAsync()
            : await query.Where(e => EF.Property<int>(e, "SectionId") == sectionId).ToListAsync();
    }

    private IQueryable<ExamBuilding> GetQuery(string[] includes)
    {
        IQueryable<ExamBuilding> query = _dbContext.Set<ExamBuilding>();
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return query;
    }
    public async Task<IEnumerable<ExamBuilding>> GetAllAsync()
        => await _dbContext.ExamBuildings.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<ExamBuilding>> GetBySectionIdAsync(int sectionId)
        => await _dbContext.ExamBuildings.Where(eb => eb.SectionId == sectionId).AsNoTracking().ToListAsync();

}