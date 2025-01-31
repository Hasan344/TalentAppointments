using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Repository
{
    public class ExamRepository : IExamRepository
    {
        private readonly MyDbContext _context;

        public ExamRepository(MyDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Exam entity)
        {
            await _context.Exams.AddAsync(entity);
            await _context.SaveChangesAsync();
        }


        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions)
        {
            var exam = await _context.Exams
            .Include(e => e.Experts) // Experts ilişkisini dahil et
            .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("Exam not found");
            }

            // İlgili SubProfessions'ı alın
            var subProfessions = await _context.SubProfessions
                .Where(sp => selectedSubProfessions.Contains(sp.Id))
                .ToListAsync();

            if (subProfessions.Count != selectedSubProfessions.Length)
            {
                throw new ArgumentException("One or more subprofessions not found.");
            }

            // SubProfessions ile ilgili uzmanları filtrele
            var experts = await _context.Experts
                .Where(e => e.SectionId == exam.SectionId &&
                            e.SubProfessions.Any(sp => selectedSubProfessions.Contains(sp.Id)))
                .ToListAsync();

            if (experts.Count < numberOfExperts)
            {
                throw new InvalidOperationException("Not enough experts available.");
            }

            // Random seçilen uzmanları ilişkilendirme
            var selectedExperts = experts
        .OrderBy(e => e.AssignmentCount) // En az atanmış olanları önce al
        .Take(numberOfExperts) // İstenen sayıda Expert seç
        .ToList();

            foreach (var expert in selectedExperts)
            {
                exam.Experts.Add(expert); // Exam'a expert ekleme işlemi
                expert.AssignmentCount++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("Exam not found");

            var allMonitors = await _context.Monitors.Where(e => e.SectionId == exam.SectionId).ToListAsync();

            if (allMonitors.Count < numberOfMonitors)
                throw new ArgumentException("Not enough experts available");

            Random rng = new Random();
            var shuffledMonitors = allMonitors.OrderBy(x => rng.Next()).Take(numberOfMonitors).ToList();

            foreach (var monitor in shuffledMonitors)
            {
                exam.Monitors.Add(monitor);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Section)
                .Include(e => e.ExamBulding)
                .Include(e => e.Commission)
                .Include(e => e.SubCommission)
                .Include(e => e.Experts)
                .Include(e => e.Monitors)
                .ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Section)
                .Include(e => e.ExamBulding)
                .Include(e => e.Commission)
                .Include(e => e.SubCommission)
                .Include(e => e.Experts)
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId)
        {
            return sectionId is null
               ? await _context.Exams
                                   .Include(e => e.Section)
                                   .Include(e => e.ExamBulding)
                                   .Include(e => e.Commission)
                                   .Include(e => e.SubCommission)
                                   .Include(e => e.Experts)
                                   .Include(e => e.Monitors)
                                   .ToListAsync()
            :    await _context.Exams
                                   .Include(e => e.Section)
                                   .Include(e => e.ExamBulding)
                                   .Include(e => e.Commission)
                                   .Include(e => e.SubCommission)
                                   .Include(e => e.Experts)
                                   .Include(e => e.Monitors)
                                   .Where(e => e.SectionId == sectionId)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId)
        {

             return _context.SubProfessions.Where(sp => sp.SectionId == sectionId).ToList();
        }

        public async Task UpdateAsync(Exam entity)
        {
            _context.Exams.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<int?> GetSectionIdByExamIdAsync(int examId)
        {
            return await _context.Exams
                .Where(e => e.Id == examId)
                .Select(e => e.SectionId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetAvailableExpertsCountAsync(int sectionId, int[] selectedSubProfessions)
        {
            return await _context.Experts
                .Where(e => e.SectionId == sectionId)
                .Where(e => e.SubProfessions.Any(sp => selectedSubProfessions.Contains(sp.Id)))
                .CountAsync();
        }
    }
}
