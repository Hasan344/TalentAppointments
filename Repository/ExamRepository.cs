using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;
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
        public async Task AddAsync(CreateExamViewModel entity)
        {
            var exam = new Exam
            {
                Name = entity.Name,
                SectionId = entity.SectionId,
                ExamBuldingId = entity.ExamBuldingId,
                ExamDate = entity.ExamDate,
                Duration = entity.Duration,
                Food = entity.Food,
                Notes = entity.Notes,
                Water = entity.Water,
                InventoryTransport = entity.InventoryTransport,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Shift = entity.Shift,
            };

            //Link selected SubProfessions
            if (entity.SelectedCommissions != null)
            {
                foreach (var commissionId in entity.SelectedCommissions)
                {
                    var commission = await _context.Commissions.FindAsync(commissionId);
                    if (commission != null)
                    {
                        // Yeni ExamCommission oluştur ve ekle
                        var examCommission = new ExamCommission
                        {
                            ExamId = exam.Id, // Exam'in Id'si otomatik olarak atanacak
                            CommissionId = commission.Id,
                            Exam = exam,
                            Commission = commission
                        };
                        exam.ExamCommissions.Add(examCommission);
                    }
                }
            }


            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }


        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions)
        {
            var exam = await _context.Exams
            .Include(e => e.Experts) // exams ilişkisini dahil et
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
        .Take(numberOfExperts) // İstenen sayıda exam seç
        .ToList();

            foreach (var expert in selectedExperts)
            {
                exam.Experts.Add(expert);
                expert.AssignmentCount++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("Exam not found");

            var allMonitors = await _context.Monitors.Where(e => e.SectionId == exam.SectionId)
                                                     .Where(e => e.Role == 2)
                                                     .Where(e => e.Gender == genderId)
                                                     .Where(e => e.BirthDate >= maxDate)
                                                     .Where(e => e.Status == 0)
                                                     .Where(e => e.Archive == 0).ToListAsync();

            if (allMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda nəzarətçi yoxdur.");

            var selectedMonitors = allMonitors
                                    .OrderBy(e => e.AssignmentCount)
                                    .Take(numberOfMonitors)
                                    .ToList();

            foreach (var monitor in selectedMonitors)
            {
                exam.Monitors.Add(monitor);
                monitor.AssignmentCount++;
            }

            await _context.SaveChangesAsync();
        }
        public async Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new Exception("Exam not found");

            var allMonitors = await _context.Monitors.Where(e => e.SectionId == exam.SectionId)
                                                     .Where(e => e.Role == 1)
                                                     .Where(e => e.Gender == genderId)
                                                     .Where(e => e.BirthDate >= maxDate)
                                                     .Where(e => e.Status == 0)
                                                     .Where(e => e.Archive == 0).ToListAsync();
            if (allMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda nəzarətçi yoxdur.");

            var selectedMonitors = allMonitors
                                    .OrderBy(e => e.AssignmentCount)
                                    .Take(numberOfMonitors)
                                    .ToList();

            foreach (var monitor in selectedMonitors)
            {
                exam.Monitors.Add(monitor);
                monitor.AssignmentCount++;
            }

            await _context.SaveChangesAsync();
        }
        public async Task AddMonitorLogAsync(MonitorLog log)
        {
            await _context.MonitorLogs.AddAsync(log);
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
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.Experts)
                .Include(e => e.Monitors)
                .ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Section)
                .Include(e => e.ExamBulding)
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
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
                                .Include(e => e.ExamCommissions)
                                    .ThenInclude(ec => ec.Commission) // Commission'ları bu şekilde include et
                                .Include(e => e.Experts)
                                .Include(e => e.Monitors)
                                .ToListAsync()
                : await _context.Exams
                                .Include(e => e.Section)
                                .Include(e => e.ExamBulding)
                                .Include(e => e.ExamCommissions)
                                    .ThenInclude(ec => ec.Commission)
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

       public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds)
        {
            // Mevcut Exam'i bul ve ilişkili verileri include et
            var existingExam = await _context.Exams
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.ExamBulding)
                .Include(e => e.Section)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            if (existingExam == null)
                throw new ArgumentException("Exam not found");

            // Exam'in özelliklerini güncelle
            existingExam.Id = exam.Id;
            existingExam.Name = exam.Name;
            existingExam.InventoryTransport = exam.InventoryTransport;
            existingExam.SectionId = exam.SectionId;
            existingExam.ExamBuldingId = exam.ExamBuldingId;
            existingExam.Duration = exam.Duration;
            existingExam.Food = exam.Food;
            existingExam.Notes = exam.Notes;
            existingExam.Water = exam.Water;
            existingExam.StartTime = exam.StartTime;
            existingExam.EndTime = exam.EndTime;
            existingExam.Shift = exam.Shift;

            // Mevcut ExamCommissions'ları temizle
            if (existingExam.ExamCommissions != null)
            {
                existingExam.ExamCommissions.Clear();
            }

            // Yeni komisyonları ekle
            if (commissionIds != null && commissionIds.Length > 0)
            {
                foreach (var commissionId in commissionIds)
                {
                    var commission = await _context.Commissions.FindAsync(commissionId);
                    if (commission != null)
                    {
                        // Yeni ExamCommission oluştur ve ekle
                        var examCommission = new ExamCommission
                        {
                            ExamId = existingExam.Id,
                            CommissionId = commission.Id,
                            Exam = existingExam,
                            Commission = commission
                        };
                        existingExam.ExamCommissions.Add(examCommission);
                    }
                }
            }

            // Değişiklikleri kaydet
            _context.Exams.Update(existingExam);
            await _context.SaveChangesAsync();
        }
    
        public async Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId)
        {
            if (sectionId == null)
            {
                return _context.Commissions.ToList();
            }
            return _context.Commissions.Where(e => e.SectionId == sectionId).ToList();
        }

        public async Task<int> GetAvailableMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate)
        {
            return await _context.Monitors
                .Where(m => m.SectionId == sectionId)
                .Where(m => m.Gender == genderId)
                .Where(m => m.BirthDate >= maxDate)
                .Where(m => m.Role == 2)
                .Where(e => e.Status == 0)
                .Where(e => e.Archive == 0)
                .CountAsync();
        }

        public async Task<int> GetAvailableHeadMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate)
        {
            return await _context.Monitors
                .Where(m => m.SectionId == sectionId)
                .Where(m => m.Gender == genderId)
                .Where(m => m.BirthDate >= maxDate)
                .Where(m => m.Role == 1)
                .Where(e => e.Status == 0)
                .Where(e => e.Archive == 0)
                .CountAsync();
        }

        public async Task AddExpertLogAsync(ExpertLog logs)
        {
            await _context.ExpertLogs.AddAsync(logs);
            await _context.SaveChangesAsync();
        }
    }
}
