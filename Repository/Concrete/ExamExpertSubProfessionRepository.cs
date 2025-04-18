using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ForQab.Repository.Concrete
{
    public class ExamExpertSubProfessionRepository : IExamExpertSubProfessionRepository
    {
        private readonly MyDbContext _context;

        public ExamExpertSubProfessionRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task<List<ExamExpertSubProfession>> GetSubProfessionsByExpertAsync(int examId, int expertId)
        {
            var query = _context.ExamExpertSubProfessions
                .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
                .ToListAsync();
            return await query;
        }

        public async Task<int> GetSubProfessionIdByExpertAsync(int examId, int expertId)
        {
            var result = await _context.ExamExpertSubProfessions
        .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
        .Select(eesp => eesp.SubProfessionId)
        .FirstOrDefaultAsync();

            if (result == null)
            {
                Console.WriteLine($"No SubProfessionId found for ExamId: {examId}, ExpertId: {expertId}");
            }
            else
            {
                Console.WriteLine($"SubProfessionId {result} found for ExamId: {examId}, ExpertId: {expertId}");
            }

            return (int)result;
        }

        public async Task RemoveSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions)
        {
            _context.ExamExpertSubProfessions.RemoveRange(subProfessions);
            await _context.SaveChangesAsync();
        }

        public async Task AddSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions)
        {
            await _context.ExamExpertSubProfessions.AddRangeAsync(subProfessions);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExamExpertSubProfession>> GetSubProfessionByExamAndExpertAsync(int examId, int expertId)
        {
            return await _context.ExamExpertSubProfessions
                .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
                .ToListAsync();
        }
        public async Task AddSubProfessionsAsync(IEnumerable<ExamExpertSubProfession> subProfessions)
        {
            _context.ChangeTracker.Clear();
            
                await _context.ExamExpertSubProfessions.AddAsync((ExamExpertSubProfession)subProfessions);
            
        }

        public async Task RemoveByExpertAsync(int examId, int expertId)
        {
            var subProfessions = await _context.ExamExpertSubProfessions
                .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
                .ToListAsync();

            _context.ExamExpertSubProfessions.RemoveRange(subProfessions);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ExamExpertSubProfession>> GetAllAsync(Expression<Func<ExamExpertSubProfession, bool>> predicate)
        {
            return await _context.ExamExpertSubProfessions.Where(predicate).ToListAsync();
        }

        public void RemoveRange(IEnumerable<ExamExpertSubProfession> entities)
        {
            _context.ExamExpertSubProfessions.RemoveRange(entities);
        }
    }

}
