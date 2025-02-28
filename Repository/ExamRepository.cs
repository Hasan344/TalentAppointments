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
                StudentCount = entity.StudentCount,
                Notes = entity.Notes,
                Water = entity.Water,
                InventoryTransport = entity.InventoryTransport,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Shift = entity.Shift,
                AdmissionTime = entity.AdmissionTime,
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
            if (entity.SelectedDegrees != null)
            {
                foreach (var degreeId in entity.SelectedDegrees)
                {
                    var degree = await _context.Degrees.FindAsync(degreeId);
                    if (degree != null)
                    {
                        // Yeni ExamCommission oluştur ve ekle
                        var examDegree = new ExamDegree
                        {
                            ExamId = exam.Id, // Exam'in Id'si otomatik olarak atanacak
                            DegreeId = degree.Id,
                            Exams = exam,
                            Degrees = degree
                        };
                        exam.ExamDegrees.Add(examDegree);
                    }
                }
            }


            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }


        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId)
        {

            var exam = await _context.Exams.Include(e => e.Experts).FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
            {
                throw new ArgumentException("Exam not found");
            }
            var federationExists = await _context.Professions.AnyAsync(p => p.Id == federationId);
            if (!federationExists)
            {
                throw new ArgumentException("Federation does not exist.");
            }

            var subProfessions = await _context.SubProfessions
                .Where(sp => selectedSubProfessions.Contains(sp.Id))
                .ToListAsync();

            if (subProfessions.Count != selectedSubProfessions.Length)
            {
                throw new ArgumentException("One or more subprofessions not found.");
            }

            var assignedExpertIds = exam.Experts.Select(ex => ex.Id).ToList(); // Önceden listeye al

            var experts = await _context.Experts
                .Where(e => e.SectionId == exam.SectionId &&
                            e.SubProfessions.Any(sp => selectedSubProfessions.Contains(sp.Id)) &&
                            !assignedExpertIds.Contains(e.Id) && // Daha iyi çevirim
                            e.Archive == 0 &&
                            e.Status == 0)
                .ToListAsync();



            if (experts.Count < numberOfExperts)
            {
                throw new InvalidOperationException("Not enough experts available.");
            }

            var selectedExperts = experts.OrderBy(e => e.AssignmentCount).Take(numberOfExperts).ToList();
            var shuffledSubProfessions = subProfessions.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 0; i < selectedExperts.Count; i++)
            {
                var expert = selectedExperts[i];
                exam.Experts.Add(expert);
                expert.AssignmentCount++;

                var assignedSubProfession = shuffledSubProfessions[i % shuffledSubProfessions.Count];

                bool existsInDatabase = await _context.ExamExpertSubProfessions
                    .AnyAsync(ees => ees.ExamId == examId &&
                                     ees.ExpertId == expert.Id &&
                                     ees.SubProfessionId == assignedSubProfession.Id &&
                                     ees.FederationId == federationId);

                bool existsInLocal = _context.ExamExpertSubProfessions.Local
                    .Any(ees => ees.ExamId == examId &&
                                ees.ExpertId == expert.Id &&
                                ees.SubProfessionId == assignedSubProfession.Id &&
                                ees.FederationId == federationId);

                if (!existsInDatabase && !existsInLocal)
                {
                    _context.ExamExpertSubProfessions.Add(new ExamExpertSubProfession
                    {
                        ExamId = examId,
                        ExpertId = expert.Id,
                        SubProfessionId = assignedSubProfession.Id,
                        FederationId = (int)federationId // Yeni eklendi
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors) // Mevcut nəzarətçiləri yüklə
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("Exam not found");

            // Zaten atanmış nəzarətçilerin Id'lerini belirleyin
            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();

            // Uygun ve daha önce atanmış olmayan nəzarətçiləri al
            var availableMonitors = await _context.Monitors
                .Where(e => e.SectionId == exam.SectionId)
                .Where(e => e.Role == 2)
                .Where(e => e.Gender == genderId)
                .Where(e => e.BirthDate >= maxDate)
                .Where(e => e.Status == 0)
                .Where(e => e.Archive == 0)
                .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id)) // Daha önce atanmışları çıkar
                .OrderBy(e => e.AssignmentCount) // Daha az atanmışları önceliklendir
                .ToListAsync();

            if (availableMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda nəzarətçi yoxdur.");

            // Belirlenen sayıda nəzarətçiyi seç
            var selectedMonitors = availableMonitors.Take(numberOfMonitors).ToList();

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

            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();

            // Head Monitors için uygun olanları al
            var allMonitors = await _context.Monitors.Where(e => e.SectionId == exam.SectionId)
                                                     .Where(e => e.Role == 1)
                                                     .Where(e => e.Gender == genderId)
                                                     .Where(e => e.BirthDate >= maxDate)
                                                     .Where(e => e.Status == 0)
                                                     .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id))
                                                     .Where(e => e.Archive == 0)
                                                     .ToListAsync();

            if (allMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda nəzarətçi yoxdur.");

            // Baş Monitorları sıraya koy
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
                .Include(e => e.ExamBuilding)
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
                .Include(e => e.ExamBuilding)
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id)) // Sadece bu imtahana aid olanlar
                    .ThenInclude(eesp => eesp.SubProfession)
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id)) // Sadece bu imtahana aid olanlar
                    .ThenInclude(eesp => eesp.Federation)
                .Include(e => e.Monitors)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(ed => ed.Degrees)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId)
        {
            return sectionId is null
                ? await _context.Exams
                                .Include(e => e.Section)
                                .Include(e => e.ExamBuilding)
                                .Include(e => e.ExamCommissions)
                                    .ThenInclude(ec => ec.Commission) // Commission'ları bu şekilde include et
                                .Include(e => e.Experts)
                                .Include(e => e.Monitors)
                                .ToListAsync()
                : await _context.Exams
                                .Include(e => e.Section)
                                .Include(e => e.ExamBuilding)
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

        public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds)
        {
            // Mevcut Exam'i bul ve ilişkili verileri include et
            var existingExam = await _context.Exams
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Section)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(e => e.Degrees)
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
            existingExam.StudentCount = exam.StudentCount;
            existingExam.AdmissionTime = exam.AdmissionTime;

            // Mevcut ExamCommissions'ları temizle
            if (existingExam.ExamCommissions != null)
            {
                existingExam.ExamCommissions.Clear();
            }
            if (existingExam.ExamDegrees != null)
            {
                existingExam.ExamDegrees.Clear();
            }
            if (commissionIds != null && commissionIds.Length > 0)
            {
                foreach (var commissionId in commissionIds)
                {
                    var commission = await _context.Commissions.FindAsync(commissionId);
                    if (commission != null)
                    {
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
            if (degreeIds != null && degreeIds.Length > 0)
            {
                foreach (var degreeId in degreeIds)
                {
                    var degree = await _context.Degrees.FindAsync(degreeId);
                    if (degree != null)
                    {
                        var examDegree = new ExamDegree
                        {
                            ExamId = existingExam.Id,
                            DegreeId = degree.Id,
                            Exams = existingExam,
                            Degrees = degree
                        };
                        existingExam.ExamDegrees.Add(examDegree);
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
        public async Task<List<int>> GetMonitorsWithLogsAsync(List<int> monitorIds)
        {
            return await _context.MonitorLogs
                                 .Where(log => monitorIds.Contains(log.SupervisorId))
                                 .Select(log => log.SupervisorId)
                                 .Distinct()
                                 .ToListAsync();
        }
        public async Task<List<int>> GetExpertsWithLogsAsync(List<int> expertIds)
        {
            return await _context.MonitorLogs
                                 .Where(log => expertIds.Contains(log.SupervisorId))
                                 .Select(log => log.SupervisorId)
                                 .Distinct()
                                 .ToListAsync();
        }
        public List<Expert> GetExpertsByExam(int examId)
        {
            return _context.Experts
                .Where(e => e.ExamExpertSubProfessions.Any(esp => esp.ExamId == examId))
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(esp => esp.SubProfession)
                .ToList();
        }
        public async Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId)
        {
            return await _context.ExamExpertSubProfessions
                .Where(esp => esp.ExamId == examId)
                .Include(esp => esp.SubProfession)
                .ToListAsync();
        }

        public async Task AssignRandomWorkersToExamAsync(int examId, int numberOfMonitors, byte workerType)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors) // Mevcut nəzarətçiləri yüklə
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("Exam not found");

            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();

            var availableMonitors = await _context.Monitors
                .Where(e => e.SectionId == exam.SectionId)
                .Where(e => e.Role == 5)
                .Where(e => e.Status == 0)
                .Where(e => e.Archive == 0)
                .Where(e => e.WorkerType == workerType)
                .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id)) 
                .OrderBy(e => e.AssignmentCount) 
                .ToListAsync();

            if (availableMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda nəzarətçi yoxdur.");

            // Belirlenen sayıda nəzarətçiyi seç
            var selectedMonitors = availableMonitors.Take(numberOfMonitors).ToList();

            foreach (var monitor in selectedMonitors)
            {
                exam.Monitors.Add(monitor);
                monitor.AssignmentCount++;
            }

            await _context.SaveChangesAsync();
        }

        //public async Task<int> GetAvailableWorkersCountAsync(int sectionId)
        //{
        //    return await _context.Monitors
        //        .Where(m => m.SectionId == sectionId)
        //        .Where(m => m.Role == 5)
        //        .Where(e => e.Status == 0)
        //        .Where(e => e.Archive == 0)
        //        .CountAsync();
        //}
    }
}
