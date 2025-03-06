using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository.Concrete
{
    public class ExamExpertSubProfessionRepository : IExamExpertSubProfessionRepository
    {
        private readonly MyDbContext _context;

        public ExamExpertSubProfessionRepository(MyDbContext context)
        {
            _context = context;
        }

        // Get all sub-professions for a particular expert in a specific exam
        public async Task<List<ExamExpertSubProfession>> GetSubProfessionsByExpertAsync(int examId, int expertId)
        {
            return await _context.ExamExpertSubProfessions
                .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
                .ToListAsync();
        }

        // Get a single sub-profession ID for a particular exam and expert
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

            return result;
        }

        // Remove multiple sub-professions
        public async Task RemoveSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions)
        {
            _context.ExamExpertSubProfessions.RemoveRange(subProfessions);
            await _context.SaveChangesAsync();
        }

        // Add multiple sub-professions
        public async Task AddSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions)
        {
            await _context.ExamExpertSubProfessions.AddRangeAsync(subProfessions);
            await _context.SaveChangesAsync();
        }

        // Get sub-profession for a given exam and expert combination (return as a list)
        public async Task<List<ExamExpertSubProfession>> GetSubProfessionByExamAndExpertAsync(int examId, int expertId)
        {
            return await _context.ExamExpertSubProfessions
                .Where(eesp => eesp.ExamId == examId && eesp.ExpertId == expertId)
                .ToListAsync();
        }
    }

}
