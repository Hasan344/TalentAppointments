using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore.Storage;
using a = DocumentFormat.OpenXml.Drawing;
using wp = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using pic = DocumentFormat.OpenXml.Drawing.Pictures;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Repository.Concrete
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
                Type = entity.Type,
                Notes = entity.Notes,
                Water = entity.Water,
                InventoryTransport = entity.InventoryTransport,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Shift = entity.Shift,
                AdmissionTime = entity.AdmissionTime,
                DistrictId = entity.DistrictId,
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
        public async Task AddAsyncForAssesment(CreateExamViewModelForAssesment entity)
        {
            var exam = new Exam
            {
                Name = entity.Name,
                SectionId = entity.SectionId,
                ExamBuldingId = entity.ExamBuldingId,
                ExamDate = entity.ExamDate,
                Type = entity.Type,
                Shift = entity.Shift,
                DistrictId = entity.DistrictId,
            };
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }
        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId, int? roomId)
        {

            var exam = await _context.Exams.Include(e => e.Experts).FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı");
            }
            var federationExists = await _context.Professions.AnyAsync(p => p.Id == federationId);
            if (!federationExists)
            {
                throw new ArgumentException("Müəssisə seçimi doğru deyil");
            }
            if (exam.SectionId == 1)
            {
                var roomExists = await _context.ExamRooms.AnyAsync(p => p.Id == roomId);
                if (!roomExists)
                {
                    throw new ArgumentException("Otaq seçimi doğru deyil.");
                }
            }
            var subProfessions = await _context.SubProfessions
                .Where(sp => selectedSubProfessions.Contains(sp.Id))
                .ToListAsync();

            if (subProfessions.Count != selectedSubProfessions.Length)
            {
                throw new ArgumentException("İxtisas seçimi doğru deyil.");
            }

            var assignedExpertIds = exam.Experts.Select(ex => ex.Id).ToList();

            var experts = await _context.Experts
                .Where(e => e.SectionId == exam.SectionId &&
                            e.ExpertsProfessions.Any(sp => selectedSubProfessions.Contains(sp.SubProfessionId)) &&
                            !assignedExpertIds.Contains(e.Id) &&
                            e.Archive == 0 &&
                            e.Status == 0 && e.Federation == federationId)
                .Include(e => e.Exams)
                .ToListAsync();

            var availableExperts = new List<Expert>();

            foreach (var expert in experts)
            {
                var isAssignedToAnotherExam = await _context.ExamExpertSubProfessions
                    .AnyAsync(ees => ees.ExpertId == expert.Id && ees.Exam.ExamDate == exam.ExamDate);

                if (!isAssignedToAnotherExam)
                {
                    availableExperts.Add(expert);
                }
            }

            if (availableExperts.Count < numberOfExperts)
            {
                throw new InvalidOperationException("Yetərli sayda ekspert yoxdur.");
            }

            var selectedExperts = availableExperts.OrderBy(e => e.AssignmentCount).Take(numberOfExperts).ToList();
            var shuffledSubProfessions = subProfessions.OrderBy(x => Guid.NewGuid()).ToList();

            for (int i = 0; i < selectedExperts.Count; i++)
            {
                var expert = selectedExperts[i];

                var isAssignedToAnotherExam = await _context.ExamExpertSubProfessions
            .AnyAsync(ees => ees.ExpertId == expert.Id && ees.Exam.ExamDate == exam.ExamDate);

                if (isAssignedToAnotherExam)
                {
                    continue;
                }

                exam.Experts.Add(expert);
                expert.AssignmentCount++;

                var assignedSubProfession = shuffledSubProfessions[i % shuffledSubProfessions.Count];

                bool existsInDatabase = await _context.ExamExpertSubProfessions
                    .AnyAsync(ees => ees.ExamId == examId &&
                                     ees.ExpertId == expert.Id &&
                                     ees.SubProfessionId == assignedSubProfession.Id &&
                                     ees.FederationId == federationId &&
                                     ees.RoomId == roomId);

                bool existsInLocal = _context.ExamExpertSubProfessions.Local
                    .Any(ees => ees.ExamId == examId &&
                                ees.ExpertId == expert.Id &&
                                ees.SubProfessionId == assignedSubProfession.Id &&
                                ees.FederationId == federationId &&
                                     ees.RoomId == roomId);

                if (!existsInDatabase && !existsInLocal)
                {
                    _context.ExamExpertSubProfessions.Add(new ExamExpertSubProfession
                    {
                        ExamId = examId,
                        ExpertId = expert.Id,
                        SubProfessionId = assignedSubProfession.Id,
                        FederationId = federationId,
                        RoomId = roomId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate, int? roomId)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .Include(e => e.ExamMonitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("İmtahan tapılmadı");

            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();
            var availableMonitors = await _context.Monitors
                .Where(e => e.SectionId == exam.SectionId)
                .Where(e => e.Role == 2)
                .Where(e => e.Status == 0)
                .Where(e => e.Archive == 0)
                .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id))
                .OrderBy(e => e.AssignmentCount)
                .ToListAsync();

            if (exam.SectionId == 1)
            {
                if (genderId == null || genderId == 0)
                {
                    availableMonitors = availableMonitors
                                        .OrderBy(e => e.AssignmentCount)
                                        .ToList();
                }
                else
                {
                    availableMonitors = availableMonitors
                    .Where(e => e.Gender == genderId)
                    .OrderBy(e => e.AssignmentCount)
                    .ToList();
                } 
                
            }

            if (exam.SectionId == 2 || exam.SectionId == 5)
            {
                availableMonitors = availableMonitors
                    .Where(e => e.District == exam.DistrictId)
                    .OrderBy(e => e.AssignmentCount)
                    .ToList();
            }

            List<Monitor> selectedMonitors = new List<Monitor>();

            foreach (var monitor in availableMonitors)
            {
                var isAssignedToAnotherExam = await _context.ExamMonitors
                    .AnyAsync(em => em.MonitorId == monitor.Id && em.Exams.ExamDate == exam.ExamDate);

                if (!isAssignedToAnotherExam)
                {
                    selectedMonitors.Add(monitor);
                }

                if (selectedMonitors.Count == numberOfMonitors)
                    break;
            }

            if (selectedMonitors.Count < numberOfMonitors)
            {
                throw new InvalidOperationException("Yetərli sayda nəzarətçi yoxdur.");
            }

            List<int> availableRooms = new List<int>();

            if (exam.SectionId == 2 || exam.SectionId == 5)
            {
                var assignedRoomIds = exam.ExamMonitors
                    .Where(m => m.RoomId.HasValue)
                    .Select(m => m.RoomId.Value)
                    .ToHashSet();

                availableRooms = await _context.ExamRooms
                    .Where(r => r.SectionId == exam.SectionId && !assignedRoomIds.Contains(r.Id))
                    .OrderBy(r => r.Id)
                    .Select(r => r.Id)
                    .ToListAsync();
            }

            for (int i = 0; i < selectedMonitors.Count; i++)
            {
                var monitor = selectedMonitors[i];

                int? assignedRoomId = ((exam.SectionId == 2 || exam.SectionId == 5) && availableRooms.Count > 0)
                    ? availableRooms.First()
                    : roomId;

                if (assignedRoomId != null && (exam.SectionId == 2 || exam.SectionId == 5))
                {
                    availableRooms.RemoveAt(0);
                }

                exam.ExamMonitors.Add(new ExamMonitor
                {
                    ExamId = examId,
                    MonitorId = monitor.Id,
                    RoomId = assignedRoomId
                });

                monitor.AssignmentCount++;
            }

            if (exam.SectionId == 2 || exam.SectionId == 5)
            {
                int additionalMonitorsCount = 0;

                if (numberOfMonitors >= 6 && numberOfMonitors <= 10)
                    additionalMonitorsCount = 1;
                else if (numberOfMonitors >= 11 && numberOfMonitors <= 21)
                    additionalMonitorsCount = 2;
                else if (numberOfMonitors >= 22)
                    additionalMonitorsCount = 3;

                var extraMonitors = availableMonitors
                    .Skip(numberOfMonitors)
                    .Take(additionalMonitorsCount)
                    .ToList();

                foreach (var monitor in extraMonitors)
                {
                    var isAssignedToAnotherExam = await _context.ExamMonitors
                        .AnyAsync(em => em.MonitorId == monitor.Id && em.Exams.ExamDate == exam.ExamDate);

                    if (!isAssignedToAnotherExam)
                    {
                        exam.ExamMonitors.Add(new ExamMonitor
                        {
                            ExamId = examId,
                            MonitorId = monitor.Id,
                            RoomId = null
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }



        public async Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new Exception("İmtahan tapılmadı");

            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();

            // Head Monitors için uygun olanları al
            var allMonitors = await _context.Monitors.Where(e => e.SectionId == exam.SectionId)
                                                     .Where(e => e.Role == 1)
                                                     .Where(e => e.Status == 0)
                                                     .Where(e => e.District == exam.DistrictId)
                                                     .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id))
                                                     .Where(e => e.Archive == 0)
                                                     .ToListAsync();

            if (allMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda rəhbər yoxdur.");

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
                .Include(e => e.District)
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
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id)) // Sadece bu imtahana aid olanlar
                    .ThenInclude(eesp => eesp.ExamRoom)
                .Include(e => e.Monitors)
                    .ThenInclude(e => e.ExamMonitors
                                          .Where(em => em.ExamId == id))
                        .ThenInclude(em => em.ExamRooms)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(ed => ed.Degrees)
                .Include(e => e.District)
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId, int type)
        {
            return sectionId is null
                ? await _context.Exams
                                .Include(e => e.Section)
                                .Include(e => e.ExamBuilding)
                                .Include(e => e.ExamCommissions)
                                    .ThenInclude(ec => ec.Commission)
                                .Include(e => e.Experts)
                                .Include(e => e.Monitors)
                                .Include(e => e.District)
                                .Where(e => e.Type == type)
                                .ToListAsync()
                : await _context.Exams
                                .Include(e => e.Section)
                                .Include(e => e.ExamBuilding)
                                .Include(e => e.ExamCommissions)
                                    .ThenInclude(ec => ec.Commission)
                                .Include(e => e.Experts)
                                .Include(e => e.Monitors)
                                .Include(e => e.District)
                                .Where(e => e.SectionId == sectionId)
                                .Where(e => e.Type == type)
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
                .Where(e => e.ExpertsProfessions.Any(sp => selectedSubProfessions.Contains(sp.SubProfessionId)))
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
                throw new ArgumentException("İmtahan tapılmadı");

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
            existingExam.DistrictId = exam.DistrictId;
            existingExam.ExamDate = exam.ExamDate;

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
        public async Task UpdateExamAsync(EditExamViewModelForAssesment exam)
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
                throw new ArgumentException("İmtahan tapılmadı");

            existingExam.Id = exam.Id;
            existingExam.Name = exam.Name;
            existingExam.SectionId = exam.SectionId;
            existingExam.ExamBuldingId = exam.ExamBuldingId;
            existingExam.Shift = exam.Shift;
            existingExam.DistrictId = exam.DistrictId;
            existingExam.ExamDate = exam.ExamDate;

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
            return await _context.ExpertLogs
                                 .Where(log => expertIds.Contains(log.ExpertId))
                                 .Select(log => log.ExpertId)
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

        public async Task AssignWorkersToExamAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı.");
            }

            var selectedWorkers = await _context.Monitors
                .Where(r => r.ExamBuildingId == exam.ExamBuldingId)
                .ToListAsync();


            foreach (var rep in selectedWorkers)
            {
                exam.Monitors.Add(rep);
            }

            await _context.SaveChangesAsync();
        }
        public async Task AssignVolunteersToExamAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .Where(e => e.SectionId == 1)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı.");
            }

            var selectedVolunteers = await _context.Monitors
                .Where(r => r.Role == 4)
                .Where(m => m.ExamBuildingId == exam.ExamBuldingId)
                .ToListAsync();


            foreach (var rep in selectedVolunteers)
            {
                exam.Monitors.Add(rep);
            }

            await _context.SaveChangesAsync();
        }
        public async Task<MemoryStream> ExportExamScheduleToWord()
        {
            var exams = await _context.Exams
                                      .Include(e => e.ExamDegrees)
                                          .ThenInclude(d => d.Degrees)  // Eğer Degrees ilişkisi varsa
                                      .Include(e => e.ExamCommissions)
                                          .ThenInclude(c => c.Commission) // Eğer Commission ilişkisi varsa
                                      .Include(e => e.ExamExpertSubProfessions)
                                          .ThenInclude(s => s.SubProfession) // Eğer SubProfession ilişkisi varsa
                                      .Include(e => e.ExamBuilding)
                                      .Include(e => e.District)
                                      .Include(e => e.Section)
                                      .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);

                // Başlık ekleme
                // Paragraph title = new Paragraph(new Run(new Text("Sınav Takvimi")));
                //title.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                //body.Append(title);

                Table table = new Table();
                TableProperties tblProp = new TableProperties(
                    new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 }
                    )
                );
                table.AppendChild(tblProp);

                TableRow headerRow = new TableRow();
                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Komissiya", "İmtahan keçirilən rayon", "İmtahan mərkəzinin adı", "İştirakçı Sayı", "Buraxılışın başlanması", "İmtahan başlanması", "İmtahanın bitməsi", "Qeyd" };
                foreach (var header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var exam in exams)
                {
                    TableRow row = new TableRow();

                    var sectionId = _context.Exams.Where(e => e.Id == exam.Id).Select(e => e.SectionId).FirstOrDefault();
                    string bgColor = "aae4e8";
                    if (sectionId == 1)
                    {
                        bgColor = "94e3a9";
                    }
                    else if (sectionId == 2)
                    {
                        bgColor = "edf2b3";
                    }
                    else if (sectionId == 3)
                    {
                        bgColor = "cdd4f7";
                    }
                    else if (sectionId == 4)
                    {
                        bgColor = "cde8f7";
                    }
                    else if (sectionId == 5)
                    {
                        bgColor = "edcacd";
                    }
                    else if (sectionId == 6)
                    {
                        bgColor = "ddf5d7";
                    }



                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );

                    row.Append(CreateColoredCell(exam.ExamDate.ToString("dd.MM.yyyy"), bgColor));
                    row.Append(CreateColoredCell(exam.Section?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamCommissions?.Select(c => c.Commission.CommissionNo) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? ""}, {exam.ExamBuilding?.Address ?? ""}", bgColor));
                    row.Append(CreateColoredCell(exam.StudentCount?.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.AdmissionTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.StartTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.EndTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell("", bgColor));

                    table.Append(row);
                }

                // Yardımcı Metot: Renklendirilmiş hücre oluşturur
                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }

                var sectionProps = new SectionProperties(
                                   new PageSize() { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape }, // A4 Landscape Boyutları
                                   new PageMargin() { Top = 720, Right = 720, Bottom = 720, Left = 720 } // Kenar boşlukları
                                   );
                body.Append(sectionProps);
                body.Append(table);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return memoryStream;

        }

        public async Task<List<DimRepresentative>> GetAvailableRepresentativesAsync()
        {
            return await _context.DimRepresentatives.OrderBy(dr => dr.Surname).Where(dr => dr.Type == 1).ToListAsync();
        }
        public async Task<List<DimRepresentative>> GetAvailableMinistryRepresentativesAsync()
        {
            return await _context.DimRepresentatives.OrderBy(dr => dr.Surname).Where(dr => dr.Type == 2).ToListAsync();
        }

        public async Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı.");
            }

            var selectedRepresentatives = await _context.DimRepresentatives
                .Where(r => selectedRepresentativeIds.Contains(r.Id))
                .Where(dr => dr.Type == 1)
                .ToListAsync();

            if (selectedRepresentatives.Count != selectedRepresentativeIds.Count)
            {
                throw new ArgumentException("Seçilmiş sayıda DİM nümayəndəsi yoxdur.");
            }

            foreach (var rep in selectedRepresentatives)
            {
                exam.Representatives.Add(rep);
            }

            await _context.SaveChangesAsync();
        }
        public async Task AssignMinistryRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı.");
            }

            var selectedRepresentatives = await _context.DimRepresentatives
                .Where(r => selectedRepresentativeIds.Contains(r.Id))
                .Where(dr => dr.Type == 2)
                .ToListAsync();

            if (selectedRepresentatives.Count != selectedRepresentativeIds.Count)
            {
                throw new ArgumentException("Seçilmiş sayıda Nazirlik nümayəndəsi yoxdur.");
            }

            foreach (var rep in selectedRepresentatives)
            {
                exam.Representatives.Add(rep);
            }

            await _context.SaveChangesAsync();
        }

        public Task<List<DataAccess.Models.Monitor>> GetAvailableWorkersAsync(int buildingId)
        {
            return _context.Monitors
                                 .Where(m => m.Role == 5)
                                 .Where(m => m.ExamBuildingId == buildingId)
                                 .OrderBy(m => m.Surname)
                                 .Include(m => m.WorkerTypeNavigation)
                                 .ToListAsync();
        }
        public async Task<Exam> GetExamWithExpertsAndSubProfessionsAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Experts)
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(e => e.SubProfession)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new Exception($"İmtahan tapılmadı : {examId}");  // 🔥 Log ekleyerek kontrol et
            }

            return exam;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Exam> GetExamWithMonitorsAsync(int examId)
        {
            return await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);
        }
        public async Task<Exam> GetExamWithRepresentativeAsync(int examId)
        {
            return await _context.Exams
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);
        }

        public async Task<byte[]> ExportExamMonitorsToWordAsync(int examId)
        {
            var exam = await _context.Exams.Include(e => e.Section)
                                           .Include(e => e.Monitors)
                                           .Include(e => e.ExamBuilding)
                                           .Where(e => e.Id == examId).FirstOrDefaultAsync();

            if (exam == null) throw new Exception("İmtahan tapılmadı");

            var monitors = await _context.ExamMonitors
                .Where(em => em.ExamId == examId)
                .Select(em => em.Monitors)
                .ToListAsync();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());




                    body.AppendChild(CreateBoldCenteredParagraph("Xüsusi qabiliyyət tələb edən ixtisaslar üzrə qabiliyyət imtahanlarında"));

                    // Second line
                    body.AppendChild(CreateBoldCenteredParagraph("İŞTİRAK EDƏN NƏZARƏTÇİLƏRİN QEYDİYYAT VƏRƏQİ"));
                    string logoPath = "wwwroot/img/State_Examination_Center_logo.svg.png";
                    AddImageToDocument(mainPart, logoPath);
                    // Direction with italic
                    Paragraph directionParagraph = new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text("Qabiliyyət istiqaməti: ")),
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }, new Italic()),
                            new Text(exam.Section?.Name?.ToString() ?? "Not specified"))
                    );
                    body.AppendChild(directionParagraph);

                    // Exam building
                    Paragraph buildingParagraph = new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }), new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text("Qabiliyyət imtahanının keçirildiyi imtahan binası:")),

                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }, new Underline { Val = UnderlineValues.Single }),
                            new Text(exam.ExamBuilding?.Name?.ToString()))
                    );
                    body.AppendChild(buildingParagraph);

                    // Line separator
                    body.AppendChild(CreateBoldCenteredParagraph("______________________________________________________________________"));

                    // Exam date
                    string day = exam.ExamDate.Day.ToString();
                    string month = new System.Globalization.CultureInfo("az-Latn-AZ").DateTimeFormat.GetMonthName(exam.ExamDate.Month).ToLower();

                    Paragraph dateParagraph = new Paragraph(
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text("İmtahan tarixi: ")),
                        new Run(new RunProperties(new Bold(), new Underline { Val = UnderlineValues.Single }, new FontSize { Val = "28" }),
                            new Text(day)),
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text("/__")),
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }, new Underline { Val = UnderlineValues.Single }),
                            new Text(month)),
                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text($"_______/{exam.ExamDate.Year}-ci il."))
                    );
                    body.AppendChild(dateParagraph);
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    // Table with specific structure
                    Table table = new Table();
                    TableProperties tblProps = new TableProperties(
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 12 },
                            new BottomBorder { Val = BorderValues.Single, Size = 12 },
                            new LeftBorder { Val = BorderValues.Single, Size = 12 },
                            new RightBorder { Val = BorderValues.Single, Size = 12 },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }));
                    table.AppendChild(tblProps);


                    TableRow headerRow1 = new TableRow();

                    TableCell cellSs = new TableCell();
                    cellSs.AppendChild(new Paragraph(new Run(new Text("S/s"))));
                    cellSs.TableCellProperties = new TableCellProperties();
                    cellSs.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellSs.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellSs);

                    // Vəzifəsi 
                    TableCell cellVezifesi = new TableCell();
                    cellVezifesi.AppendChild(new Paragraph(new Run(new Text("Vəzifəsi (imtahan zalı, məşq zalı)"))));
                    cellVezifesi.TableCellProperties = new TableCellProperties();
                    cellVezifesi.TableCellProperties.AppendChild(new GridSpan() { Val = 2 });
                    cellVezifesi.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellVezifesi);

                    // Soyadı, adı, ata adı
                    TableCell cellName = new TableCell();
                    cellName.AppendChild(new Paragraph(new Run(new Text("Soyadı, adı, ata adı"))));
                    cellName.TableCellProperties = new TableCellProperties();
                    cellName.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellName.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellName);

                    // İmza / I növbə
                    TableCell cellImza1 = new TableCell();
                    cellImza1.AppendChild(new Paragraph(new Run(new Text("İmza / I növbə"))));
                    cellImza1.TableCellProperties = new TableCellProperties();
                    cellImza1.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellImza1.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellImza1);

                    // İmza / II növbə
                    TableCell cellImza2 = new TableCell();
                    cellImza2.AppendChild(new Paragraph(new Run(new Text("İmza / II növbə"))));
                    cellImza2.TableCellProperties = new TableCellProperties();
                    cellImza2.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellImza2.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellImza2);

                    table.AppendChild(headerRow1);

                    TableRow headerRow2 = new TableRow();

                    TableCell cellSsContinue = new TableCell();
                    cellSsContinue.AppendChild(new Paragraph());
                    cellSsContinue.TableCellProperties = new TableCellProperties();
                    cellSsContinue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellSsContinue);

                    // I növbə
                    TableCell cellInnovbe = new TableCell();
                    cellInnovbe.AppendChild(new Paragraph(new Run(new Text("I növbə"))));
                    cellInnovbe.TableCellProperties = new TableCellProperties();
                    cellInnovbe.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow2.AppendChild(cellInnovbe);

                    // II növbə
                    TableCell cellIInnovbe = new TableCell();
                    cellIInnovbe.AppendChild(new Paragraph(new Run(new Text("II növbə"))));
                    cellIInnovbe.TableCellProperties = new TableCellProperties();
                    cellIInnovbe.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow2.AppendChild(cellIInnovbe);

                    // Continued cell for Soyadı, adı, ata adı
                    TableCell cellNameContinue = new TableCell();
                    cellNameContinue.AppendChild(new Paragraph());
                    cellNameContinue.TableCellProperties = new TableCellProperties();
                    cellNameContinue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellNameContinue);

                    // Continued cell for İmza / I növbə
                    TableCell cellImza1Continue = new TableCell();
                    cellImza1Continue.AppendChild(new Paragraph());
                    cellImza1Continue.TableCellProperties = new TableCellProperties();
                    cellImza1Continue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellImza1Continue);

                    // Continued cell for İmza / II növbə
                    TableCell cellImza2Continue = new TableCell();
                    cellImza2Continue.AppendChild(new Paragraph());
                    cellImza2Continue.TableCellProperties = new TableCellProperties();
                    cellImza2Continue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellImza2Continue);

                    table.AppendChild(headerRow2);

                    var monitorCount = monitors.Count;

                    // Data rows
                    for (int i = 0; i < monitorCount; i++)
                    {
                        TableRow dataRow = new TableRow();

                        // Row number cell
                        TableCell cellRowNum = new TableCell();
                        cellRowNum.AppendChild(new Paragraph(new Run(new Text($"{i + 1}."))));
                        dataRow.AppendChild(cellRowNum);

                        TableCell cellLoc1 = new TableCell();
                        cellLoc1.AppendChild(new Paragraph(new Run(new Text(""))));
                        dataRow.AppendChild(cellLoc1);

                        TableCell cellLoc2 = new TableCell();
                        cellLoc2.AppendChild(new Paragraph(new Run(new Text(""))));
                        dataRow.AppendChild(cellLoc2);

                        TableCell cellMonitorName = new TableCell();
                        if (monitors[i] != null)
                        {
                            string surname = monitors[i].Surname ?? "";
                            string name = monitors[i].Name ?? "";
                            string fName = monitors[i].Fname ?? "";
                            string fullName = $"{surname} {name} {fName}";
                            cellMonitorName.AppendChild(new Paragraph(new Run(new Text(fullName))));
                        }
                        else
                        {
                            cellMonitorName.AppendChild(new Paragraph());
                        }
                        dataRow.AppendChild(cellMonitorName);

                        TableCell cellSig1 = new TableCell();
                        cellSig1.AppendChild(new Paragraph());
                        dataRow.AppendChild(cellSig1);

                        TableCell cellSig2 = new TableCell();
                        cellSig2.AppendChild(new Paragraph());
                        dataRow.AppendChild(cellSig2);

                        table.AppendChild(dataRow);
                    }

                    body.AppendChild(table);
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    Paragraph noteParagraph = new Paragraph(
                        new Run(new RunProperties(new Italic()),
                            new Text("Qeyd. İmtahana gəlməyən iştirakçının qarşısında (imza bölməsində) gəlmədi yazılır."))
                    );
                    body.AppendChild(noteParagraph);
                    body.AppendChild(new Paragraph(new Run(new Text(""))));

                    body.AppendChild(CreateBoldParagraph("İmtahan günü üçün məsul şəxs: ________/__________________________________/"));
                    body.AppendChild(CreateParagraph("                                             (imza)          (soyadı, adı, atasının adı)"));

                    mainPart.Document.Save();
                }
                return memoryStream.ToArray();
            }
        }

        private static Paragraph CreateBoldCenteredParagraph(string text)
        {
            return new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                new Run(new RunProperties(
                    new Bold(),
                    new RunFonts { Ascii = "Calibri", EastAsia = "Calibri", ComplexScript = "Calibri" },
                    new FontSize { Val = "28" }),
                    new Text(text)
                )
            );
        }

        private static Paragraph CreateBoldParagraph(string text)
        {
            return new Paragraph(
                new Run(new RunProperties(
                    new Bold(),
                    new RunFonts { Ascii = "Calibri", EastAsia = "Calibri", ComplexScript = "Calibri" },
                    new FontSize { Val = "28" }),
                    new Text(text)
                )
            );
        }

        private static Paragraph CreateParagraph(string text)
        {
            return new Paragraph(
                new Run(new RunProperties(
                    new RunFonts { Ascii = "Calibri", EastAsia = "Calibri", ComplexScript = "Calibri" },
                    new FontSize { Val = "28" }),
                    new Text(text)
                )
            );
        }
        private void AddImageToDocument(MainDocumentPart mainPart, string imagePath)
        {
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (FileStream stream = new FileStream(imagePath, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            // Image ilişkilendirmesi için ID al
            string relationshipId = mainPart.GetIdOfPart(imagePart);

            // Resmi ekle
            var element =
                new Drawing(
                    new wp.Inline(
                        new wp.Extent { Cx = 990000L, Cy = 792000L }, // Resmin boyutu (ayarlanabilir)
                        new wp.EffectExtent
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new wp.DocProperties { Id = 1U, Name = "Logo Image" },
                        new wp.NonVisualGraphicFrameDrawingProperties(
                            new a.GraphicFrameLocks { NoChangeAspect = true }
                        ),
                        new a.Graphic(
                            new a.GraphicData(
                                new pic.Picture(
                                    new pic.NonVisualPictureProperties(
                                        new pic.NonVisualDrawingProperties { Id = 0U, Name = "Logo.png" },
                                        new pic.NonVisualPictureDrawingProperties()
                                    ),
                                    new pic.BlipFill(
                                        new a.Blip { Embed = relationshipId },
                                        new a.Stretch(new a.FillRectangle())
                                    ),
                                    new pic.ShapeProperties(
                                        new a.Transform2D(
                                            new a.Offset { X = 0L, Y = 0L },
                                            new a.Extents { Cx = 990000L, Cy = 792000L }
                                        ),
                                        new a.PresetGeometry(new a.AdjustValueList()) { Preset = a.ShapeTypeValues.Rectangle }
                                    )
                                )
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                        )
                    )
                    { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U }
                );

            // Paragraf oluşturarak en üste ekle
            Paragraph imageParagraph = new Paragraph(new Run(element));
            mainPart.Document.Body.InsertAt(imageParagraph, 0);
        }

        public async Task AssignExpertsForMXToExamAsync(AssignExpertForMXToExamViewModel viewModel)
        {
            var exam = await _context.Exams
                .Include(e => e.Experts)
                .FirstOrDefaultAsync(e => e.Id == viewModel.ExamId);

            if (exam == null) return;

            var examExpertList = viewModel.ExpertForms
                .Select(expertForm => new ExamExpertSubProfession
                {
                    ExpertId = expertForm.ExpertId,
                    ExamId = viewModel.ExamId,
                    RoomId = expertForm.RoomId
                })
                .ToList();

            foreach (var expertForm in viewModel.ExpertForms)
            {
                if (!exam.Experts.Any(e => e.Id == expertForm.ExpertId))
                {
                    var expert = await _context.Experts.FindAsync(expertForm.ExpertId);
                    if (expert != null)
                    {
                        exam.Experts.Add(expert);
                    }
                }
            }

            await _context.ExamExpertSubProfessions.AddRangeAsync(examExpertList);
            await _context.SaveChangesAsync();
        }



        public async Task AssignMonitorsForMXToExamAsync(AssignMonitorForMXToExamViewModel viewModel)
        {
            var examMonitorList = viewModel.MonitorForms
                .Select(monitorForm => new ExamMonitor
                {
                    MonitorId = monitorForm.MonitorId,
                    ExamId = viewModel.ExamId,
                    RoomId = monitorForm.RoomId
                })
                .ToList();

            await _context.ExamMonitors.AddRangeAsync(examMonitorList);
            await _context.SaveChangesAsync();
        }
        public async Task AssignWorkersForMXToExamAsync(AssignWorkerForMXToExamViewModel viewModel)
        {
            var examMonitorList = viewModel.MonitorForms
                .Select(monitorForm => new ExamMonitor
                {
                    MonitorId = monitorForm.MonitorId,
                    ExamId = viewModel.ExamId
                })
                .ToList();


            await _context.ExamMonitors.AddRangeAsync(examMonitorList);
            await _context.SaveChangesAsync();
        }


    }
}