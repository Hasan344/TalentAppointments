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
using DocumentFormat.OpenXml.Bibliography;
using ForQab.Service.Helpers;

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
                burK = entity.burK,
                burQ = entity.burQ,
                Stekan = entity.Stekan,
            };

            if (entity.SelectedCommissions != null)
            {
                foreach (var commissionId in entity.SelectedCommissions)
                {
                    var commission = await _context.Commissions.FindAsync(commissionId);
                    if (commission != null)
                    {
                        var examCommission = new ExamCommission
                        {
                            ExamId = exam.Id,
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
                        var examDegree = new ExamDegree
                        {
                            ExamId = exam.Id,
                            DegreeId = degree.Id,
                            Exams = exam,
                            Degrees = degree
                        };
                        exam.ExamDegrees.Add(examDegree);
                    }
                }
            }
            if (entity.SelectedSubjects != null)
            {
                foreach (var subjectId in entity.SelectedSubjects)
                {
                    var subject = await _context.Subjects.FindAsync(subjectId);
                    if (subject != null)
                    {
                        var examSubject = new ExamSubject
                        {
                            ExamId = exam.Id,      // 0 here; EF fixes it up via the nav below
                            SubjectId = subject.Id,
                            Exams = exam,
                            Subjects = subject
                        };
                        exam.ExamSubjects.Add(examSubject);
                    }
                }
            }

            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }
        public async Task<Exam?> GetTrackedByIdAsync(int examId)
        {
            return await _context.Exams
                .AsSplitQuery()
                .Include(e => e.Monitors)
                .Include(e => e.Experts)
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);
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
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
            };
            await _context.Exams.AddAsync(exam);
            await _context.SaveChangesAsync();
        }
        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId, int? roomId, int[] seed, string? userName = null)
        {
            seed = SeededSelector.Validate(seed);

            var exam = await _context.Exams.Include(e => e.Experts).FirstOrDefaultAsync(e => e.Id == examId);
            if (exam == null)
                throw new ArgumentException("İmtahan tapılmadı");

            // Federation yalnız verilibsə (>0) yoxlanılır — Section 3/4/6 federationsuz işləyir.
            bool hasFederation = federationId > 0;
            int? fedToWrite = hasFederation ? federationId : (int?)null;

            if (hasFederation)
            {
                var federationExists = await _context.Professions.AnyAsync(p => p.Id == federationId);
                if (!federationExists)
                    throw new ArgumentException("Müəssisə seçimi doğru deyil");
            }

            if (exam.SectionId == 1)
            {
                var roomExists = await _context.ExamRooms.AnyAsync(p => p.Id == roomId);
                if (!roomExists)
                    throw new ArgumentException("Otaq seçimi doğru deyil.");
            }

            var subProfessions = await _context.SubProfessions
                .Where(sp => selectedSubProfessions.Contains(sp.Id))
                .ToListAsync();

            if (subProfessions.Count != selectedSubProfessions.Length)
                throw new ArgumentException("İxtisas seçimi doğru deyil.");

            var assignedExpertIds = exam.Experts.Select(ex => ex.Id).ToList();

            var availableExperts = await _context.Experts
                .Where(e => e.SectionId == exam.SectionId &&
                            e.ExpertsProfessions.Any(sp => selectedSubProfessions.Contains(sp.SubProfessionId)) &&
                            !assignedExpertIds.Contains(e.Id) &&
                            e.Archive == 0 &&
                            e.Status == 0 &&
                            (!hasFederation || e.Federation == federationId) &&   // ← federation opsiyonel
                            !_context.ExamExpertSubProfessions
                                .Any(ees => ees.ExamId == examId && ees.ExpertId == e.Id) &&
                            !_context.ExamExpertSubProfessions
                                .Any(ees => ees.ExpertId == e.Id && ees.Exam.ExamDate == exam.ExamDate))
                .Include(e => e.Exams)
                .Include(e => e.ExamExpertSubProfessions)
                    .ThenInclude(ees => ees.Exam)
                .AsSplitQuery()
                .ToListAsync();

            if (availableExperts.Count < numberOfExperts)
                throw new InvalidOperationException("Yetərli sayda ekspert yoxdur.");

            var orderedExperts = SeededSelector.Order(availableExperts, seed, e => e.Id, e => e.ThisYearAssignmentCount);
            var selectedExperts = orderedExperts.Take(numberOfExperts).ToList();
            var shuffledSubProfessions = SeededSelector.Order(subProfessions, seed, sp => sp.Id, _ => 0);

            // N+1 sorğularını döngüdən çıxarırıq (pre-loaded HashSet)
            var selectedExpertIds = selectedExperts.Select(e => e.Id).ToList();

            var assignedOnSameDateSet = (await _context.ExamExpertSubProfessions
                    .Where(ees => selectedExpertIds.Contains(ees.ExpertId) && ees.Exam.ExamDate == exam.ExamDate)
                    .Select(ees => ees.ExpertId).Distinct().ToListAsync())
                .ToHashSet();

            var existingAssignmentSet = (await _context.ExamExpertSubProfessions
                    .Where(ees => ees.ExamId == examId)
                    .Select(ees => new { ees.ExpertId, ees.SubProfessionId, ees.FederationId, ees.RoomId })
                    .ToListAsync())
                .Select(a => (a.ExpertId, (int?)a.SubProfessionId, (int?)a.FederationId, a.RoomId))
                .ToHashSet();

            var actuallyAssigned = new List<int>();

            for (int i = 0; i < selectedExperts.Count; i++)
            {
                var expert = selectedExperts[i];

                if (assignedOnSameDateSet.Contains(expert.Id))
                    continue;

                exam.Experts.Add(expert);
                actuallyAssigned.Add(expert.Id);

                var assignedSubProfession = shuffledSubProfessions[i % shuffledSubProfessions.Count];

                bool existsInDatabase = existingAssignmentSet.Contains(
                    (expert.Id, (int?)assignedSubProfession.Id, fedToWrite, roomId));   // ← fedToWrite

                bool existsInLocal = _context.ExamExpertSubProfessions.Local
                    .Any(ees => ees.ExamId == examId &&
                                ees.ExpertId == expert.Id &&
                                ees.SubProfessionId == assignedSubProfession.Id &&
                                ees.FederationId == fedToWrite &&                        // ← fedToWrite
                                ees.RoomId == roomId);

                if (!existsInDatabase && !existsInLocal)
                {
                    _context.ExamExpertSubProfessions.Add(new ExamExpertSubProfession
                    {
                        ExamId = examId,
                        ExpertId = expert.Id,
                        SubProfessionId = assignedSubProfession.Id,
                        FederationId = fedToWrite,                                       // ← fedToWrite (Section 3 → null)
                        RoomId = roomId
                    });
                }
            }

            _context.AssignmentSeedLogs.Add(new AssignmentSeedLog
            {
                ExamId = examId,
                AssignmentType = 1,
                Seed1 = seed[0],
                Seed2 = seed[1],
                Seed3 = seed[2],
                NumberRequested = numberOfExperts,
                Parameters = $"federationId={federationId};roomId={roomId};subProf={string.Join('|', selectedSubProfessions)}",
                CandidatePool = SeededSelector.SerializePool(availableExperts, e => e.Id, e => e.ThisYearAssignmentCount),
                SelectedIds = SeededSelector.SerializeIds(actuallyAssigned),
                UserName = userName,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        public async Task AssignRandomMonitorsToExamAsync(
            int examId,
            int numberOfMonitors,
            int? genderId,
            DateOnly? maxDate,
            int? roomId)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .Include(e => e.ExamMonitors)
                .AsSplitQuery()                                  // ← iki koleksiyon üçün cartesian explosion-u önləyir
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
                throw new ArgumentException("İmtahan tapılmadı");

            var alreadyAssignedMonitorIds = exam.Monitors.Select(m => m.Id).ToHashSet();

            // -------------------------------------------------------
            // DÜZƏLİŞ: OrderBy(e => e.ThisYearAssignmentCount) SQL-dən
            // silindi — bu [NotMapped] property EF Core-da translate
            // olunmur. Əvəzinə ToListAsync()-dən sonra in-memory sıralama.
            // -------------------------------------------------------
            var availableMonitors = await _context.Monitors
                .Include(e => e.ExamMonitors)
                    .ThenInclude(em => em.Exams)          // ThisYearAssignmentCount üçün lazımdır
                .Where(e => e.SectionId == exam.SectionId)
                .Where(e => e.Role == 2)
                .Where(e => (int?)e.Status == 0)
                .Where(e => (int)e.Archive == 0)
                .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id))
                .ToListAsync();                            // ← client-side-a keçirik

            // İndi in-memory olaraq ThisYearAssignmentCount-a görə sırala
            availableMonitors = availableMonitors
                .OrderBy(e => e.ThisYearAssignmentCount)
                .ToList();

            // Section 1: cinsiyyət + maksimum doğum tarixi filtrləri
            if (exam.SectionId == 1)
            {
                if (genderId != null && genderId != 0)
                {
                    availableMonitors = availableMonitors
                        .Where(e => e.Gender == genderId)
                        .OrderBy(e => e.ThisYearAssignmentCount)
                        .ToList();
                }

                if (maxDate.HasValue && maxDate.Value != default)
                {
                    availableMonitors = availableMonitors
                        .Where(e => e.BirthDate == null || e.BirthDate >= maxDate.Value)
                        .ToList();
                }
            }

            availableMonitors = availableMonitors
                .Where(e => e.District == exam.DistrictId)
                .OrderBy(e => e.ThisYearAssignmentCount)
                .ToList();

            // ── N+1 sorğusunu döngüdən çıxarırıq (DB SaveChanges-ə qədər dəyişmir) ──
            // Eyni imtahan tarixində artıq təyin olunmuş nəzarətçi ID-ləri — tək sorğu.
            var monitorIdsToCheck = availableMonitors.Select(m => m.Id).ToList();
            var assignedMonitorIdsOnSameDate = (await _context.ExamMonitors
                    .Where(em => monitorIdsToCheck.Contains(em.MonitorId) && em.Exams.ExamDate == exam.ExamDate)
                    .Select(em => em.MonitorId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();
            // ────────────────────────────────────────────────────────────────────────

            var selectedMonitors = new List<Monitor>();

            foreach (var monitor in availableMonitors)
            {
                var isAssignedToAnotherExam = assignedMonitorIdsOnSameDate.Contains(monitor.Id);

                if (!isAssignedToAnotherExam)
                {
                    selectedMonitors.Add(monitor);
                }

                if (selectedMonitors.Count == numberOfMonitors)
                    break;
            }

            if (selectedMonitors.Count < numberOfMonitors)
            {
                throw new InvalidOperationException("Yetərli sayda nəzarətçi yoxdur. " +
                    $"Tələb: {numberOfMonitors}, mövcud: {selectedMonitors.Count}");
            }

            // Otaq təyinatı (Section 2 / 5)
            var availableRooms = new List<int>();

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
            }

            // Section 2/5: əlavə ehtiyat nəzarətçilər
            if (exam.SectionId == 2 || exam.SectionId == 5)
            {
                int additionalMonitorsCount = numberOfMonitors switch
                {
                    >= 6 and <= 10 => 1,
                    >= 11 and <= 21 => 2,
                    >= 22 => 3,
                    _ => 0
                };

                var extraMonitors = availableMonitors
                    .Skip(numberOfMonitors)
                    .Take(additionalMonitorsCount)
                    .ToList();

                foreach (var monitor in extraMonitors)
                {
                    // extraMonitors zaten availableMonitors-dandır → eyni HashSet kifayətdir
                    var isAssignedToAnotherExam = assignedMonitorIdsOnSameDate.Contains(monitor.Id);

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

            var allMonitors = await _context.Monitors
                .Include(e => e.ExamMonitors)
                    .ThenInclude(em => em.Exams)
                .Where(e => e.SectionId == exam.SectionId)
                .Where(e => e.Role == 1)
                .Where(e => e.Status == 0)
                .Where(e => e.District == exam.DistrictId)
                .Where(e => !alreadyAssignedMonitorIds.Contains(e.Id))
                .Where(e => e.Archive == 0)
                .ToListAsync();

            if (allMonitors.Count < numberOfMonitors)
                throw new Exception("Yeterli sayda rəhbər yoxdur.");

            var random = new Random();

            var selectedMonitors = allMonitors
                .GroupBy(m => m.ThisYearAssignmentCount)
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderBy(_ => random.Next()))
                .Take(numberOfMonitors)
                .ToList();

            foreach (var monitor in selectedMonitors)
            {
                exam.Monitors.Add(monitor);
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
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Section)
                .Include(e => e.ExamBuilding)
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id))
                    .ThenInclude(eesp => eesp.SubProfession)
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id))
                    .ThenInclude(eesp => eesp.Federation)
                .Include(e => e.Experts)
                    .ThenInclude(ex => ex.ExamExpertSubProfessions
                        .Where(eesp => eesp.ExamId == id))
                    .ThenInclude(eesp => eesp.ExamRoom)
                .Include(e => e.Monitors)
                    .ThenInclude(e => e.ExamMonitors
                        .Where(em => em.ExamId == id))
                    .ThenInclude(em => em.ExamRooms)
                .Include(e => e.Monitors)
                    .ThenInclude(m => m.WorkerTypeNavigation)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(ed => ed.Degrees)
                .Include(e => e.District)
                .Include(e => e.Representatives)
                .Include(e => e.ExamSubjects)
                    .ThenInclude(e => e.Subjects)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(
    int? sectionId, int type, int? year, int? examBuildingId = null)
        {
            var query = _context.Exams
                .AsNoTracking()
                .Include(e => e.Section)
                .Include(e => e.ExamBuilding)
                .Include(e => e.ExamCommissions).ThenInclude(ec => ec.Commission)
                .Include(e => e.ExamExpertSubProfessions)
                .Include(e => e.District)
                .Where(e => e.Type == type);
            // Experts və Monitors silindi — Index-də göstərilmir,
            // amma hər biri çox böyük JOIN yaradırdı

            if (sectionId.HasValue)
                query = query.Where(e => e.SectionId == sectionId);

            if (examBuildingId.HasValue)
                query = query.Where(e => e.ExamBuldingId == examBuildingId);

            if (year.HasValue && year != 0)
                query = query.Where(e => e.ExamDate.Year == year);

            return await query
                .OrderByDescending(e => e.ExamDate)
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

        public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds, int[] selectedSubjects)
        {
            var existingExam = await _context.Exams
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Section)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(e => e.Degrees)
                .Include(e => e.ExamSubjects)
                    .ThenInclude(e => e.Subjects)
                .FirstOrDefaultAsync(e => e.Id == exam.Id);

            if (existingExam == null)
                throw new ArgumentException("İmtahan tapılmadı");

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
            existingExam.burQ = exam.burQ;
            existingExam.burK = exam.burK;
            existingExam.Stekan = exam.Stekan;

            if (existingExam.ExamCommissions != null)
            {
                existingExam.ExamCommissions.Clear();
            }
            if (existingExam.ExamDegrees != null)
            {
                existingExam.ExamDegrees.Clear();
            }
            if (existingExam.ExamSubjects != null)
            {
                existingExam.ExamSubjects.Clear();
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
            if (selectedSubjects != null && selectedSubjects.Length > 0)
            {
                foreach (var subjectId in selectedSubjects)
                {
                    var subject = await _context.Subjects.FindAsync(subjectId);
                    if (subject != null)
                    {
                        var examSubject = new ExamSubject
                        {
                            ExamId = existingExam.Id,
                            SubjectId = subject.Id,
                            Exams = existingExam,
                            Subjects = subject
                        };
                        existingExam.ExamSubjects.Add(examSubject);
                    }
                }
            }

            _context.Exams.Update(existingExam);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateExamAsync(EditExamViewModelForAssesment exam)
        {
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
            existingExam.StartTime = exam.StartTime;
            existingExam.EndTime = exam.EndTime;

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
                .Where(r => r.Archive == 0)
                .Where(r => r.Status == 0)
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
        public async Task<MemoryStream> ExportExamScheduleToWord(int? year)
        {
            var query = _context.Exams
                        .Include(e => e.ExamDegrees)
                            .ThenInclude(d => d.Degrees)
                        .Include(e => e.ExamCommissions)
                            .ThenInclude(c => c.Commission)
                        .Include(e => e.ExamExpertSubProfessions)
                            .ThenInclude(s => s.SubProfession)
                        .Include(e => e.ExamBuilding)
                        .Include(e => e.District)
                        .Include(e => e.Section)
                        .Include(e => e.ExamSubjects)
                            .ThenInclude(e => e.Subjects)
                        .Where(e => e.Type == 1);

            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(e => e.ExamDate.Year == year.Value);
            }

            var exams = await query.OrderBy(e => e.ExamDate)
                                   .ThenBy(e => e.SectionId)
                                   .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);


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

                TableRow headerRow = new TableRow(new TableRowProperties(
                                                      new TableHeader()
                                                  ));
                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Təhsil səviyyəsi", "Komissiya", "İmtahan fənləri", "İmtahan keçirilən şəhər(rayon)", "İmtahan mərkəzinin adı və ünvanı", "İştirakçı Sayı", "Buraxılışın başlanması", "İmtahan başlanması", "İmtahanın bitməsi", "Qeyd" };
                foreach (var header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var exam in exams)
                {
                    TableRow row = new TableRow(
                                            new TableRowProperties(
                                                new CantSplit()
                                            )
                                        );

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
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(c => c.Degrees.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamCommissions?
                                                        .Select(c => $"{c.Commission.CommissionNo} - {c.Commission.Name}")
                                                        ?? new List<string>()),
                                                    bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamSubjects?.Select(c => c.Subjects.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? ""}, {exam.ExamBuilding?.Address ?? ""}", bgColor));
                    row.Append(CreateColoredCell(exam.StudentCount?.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.AdmissionTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.StartTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.EndTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell("", bgColor));

                    table.Append(row);
                }

                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }

                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);

                var paragraph = new Paragraph();
                paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                paragraph.Append(new Run(
                    new RunProperties(new NoProof()),
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                paragraph.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                ));
                paragraph.Append(new Run(new Text("1"))); // Placeholder
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                Footer footer = new Footer(paragraph);
                footerPart.Footer = footer;
                footerPart.Footer.Save();

                var sectionProps = new SectionProperties(
                    new PageSize() { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                    new PageMargin() { Top = 720, Right = 720, Bottom = 720, Left = 720 },
                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId }
                );

                body.Append(sectionProps);
                body.Append(table);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return memoryStream;

        }
        public async Task<MemoryStream> ExportExamScheduleToWordForSeason(int? year)
        {
            var query = _context.Exams
                        .Include(e => e.ExamDegrees)
                            .ThenInclude(d => d.Degrees)
                        .Include(e => e.ExamCommissions)
                            .ThenInclude(c => c.Commission)
                        .Include(e => e.ExamExpertSubProfessions)
                            .ThenInclude(s => s.SubProfession)
                        .Include(e => e.ExamBuilding)
                        .Include(e => e.District)
                        .Include(e => e.Section)
                        .Include(e => e.ExamSubjects)
                            .ThenInclude(e => e.Subjects)
            .Where(e => e.Type == 1);

            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(e => e.ExamDate.Year == year.Value);
            }

            var exams = await query.OrderBy(e => e.ExamDate)
                                   .ThenBy(e => e.SectionId)
                                   .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);


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

                TableRow headerRow = new TableRow(new TableRowProperties(
                                                      new TableHeader()
                                                  ));
                string[] headers = { "Təhsil səviyyəsi", "İmtahan Tarixi", "İmtahan keçirilən şəhər(rayon)", "İştirakçı Sayı", "Binaların sayı", "İmtahan rəhbərlərinin sayı", "Zal nəzarətçilərinin sayı", "Ekspert sayı", "BR əməkdaşı" };
                foreach (var header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var exam in exams)
                {
                    TableRow row = new TableRow(
                                            new TableRowProperties(
                                                new CantSplit()
                                            )
                                        );

                    var sectionId = _context.Exams.Where(e => e.Id == exam.Id).Select(e => e.SectionId).FirstOrDefault();
                    string bgColor = "ffffff";
                    var role1MonitorCount = exam.ExamMonitors?
                                                .Count(em => em.Monitors != null && em.Monitors.Role == 1) ?? 0;
                    var role2MonitorCount = exam.ExamMonitors?
                                                .Count(em => em.Monitors != null && em.Monitors.Role == 2) ?? 0;
                    var role1ExpertCount = exam.ExamExperts?
                                                .Count(em => em.Experts != null && em.Experts.Kons == false) ?? 0;


                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );

                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(c => c.Degrees.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.ExamDate.ToString("dd.MM.yyyy"), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.StudentCount?.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.ExamBuilding?.Name.Count().ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(role1MonitorCount.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(role2MonitorCount.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(role1ExpertCount.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell("", bgColor));

                    table.Append(row);
                }

                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }

                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);

                var paragraph = new Paragraph();
                paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                paragraph.Append(new Run(
                    new RunProperties(new NoProof()),
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                paragraph.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                ));
                paragraph.Append(new Run(new Text("1"))); // Placeholder
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                Footer footer = new Footer(paragraph);
                footerPart.Footer = footer;
                footerPart.Footer.Save();

                var sectionProps = new SectionProperties(
                    new PageSize() { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                    new PageMargin() { Top = 720, Right = 720, Bottom = 720, Left = 720 },
                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId }
                );

                body.Append(sectionProps);
                body.Append(table);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return memoryStream;

        }

        public async Task<MemoryStream> ExportExamCalendarToWord(int? year)
        {
            var query = _context.Exams
                        .Include(e => e.ExamDegrees)
                            .ThenInclude(d => d.Degrees)
                        .Include(e => e.ExamCommissions)
                            .ThenInclude(c => c.Commission)
                        .Include(e => e.ExamExpertSubProfessions)
                            .ThenInclude(s => s.SubProfession)
                        .Include(e => e.ExamBuilding)
                        .Include(e => e.District)
                        .Include(e => e.Section)
                        .Include(e => e.ExamSubjects)
                            .ThenInclude(e => e.Subjects)
                        .Where(e => e.Type == 1);

            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(e => e.ExamDate.Year == year.Value);
            }

            var exams = await query.OrderBy(e => e.SectionId)
                                   .ThenBy(e => e.ExamDate)
                                   .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);

                Paragraph topTitle = new Paragraph(
    new ParagraphProperties(
        new Justification() { Val = JustificationValues.Center },
        new SpacingBetweenLines() { After = "200" }
    ),
    new Run(
        new RunProperties(new Bold(), new FontSize() { Val = "28" }),
        new Text("Qabiliyyət imtahanlarının 11 iyun 2026-cı il tarixinə olan qrafiki haqqında") { Space = SpaceProcessingModeValues.Preserve },
        new Break(),
        new Text("Məlumat")
    )
);
                body.Append(topTitle);

                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Təhsil səviyyəsi", "Komissiya", "İmtahan fənləri", "İmtahan keçirilən şəhər(rayon)", "İmtahan mərkəzinin adı və ünvanı" };

                // Hər section üçün ayrı başlıq + ayrı cədvəl
                var sectionGroups = exams.GroupBy(e => e.SectionId);

                foreach (var group in sectionGroups)
                {
                    int sectionId = group.Key;
                    string sectionName = group.First().Section?.Name ?? "";



                    // Section başlığı: "<İstiqamət adı> üzrə imtahanlar"
                    Paragraph sectionTitle = new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "200", After = "120" }
                        ),
                        new Run(
                            new RunProperties(new Bold(), new FontSize() { Val = "28" }),
                            new Text($"{sectionName} üzrə imtahanlar")
                        )
                    );
                    body.Append(sectionTitle);

                    // Section-a uyğun fon rəngi (1–2 ton açıq)
                    string bgColor = "c3ecef";
                    switch (sectionId)
                    {
                        case 1: bgColor = "b4ebc3"; break;
                        case 2: bgColor = "f2f6ca"; break;
                        case 3: bgColor = "dce1f9"; break;
                        case 4: bgColor = "dceff9"; break;
                        case 5: bgColor = "f2dadc"; break;
                        case 6: bgColor = "e7f8e3"; break;
                    }

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

                    TableRow headerRow = new TableRow(new TableRowProperties(new TableHeader()));
                    foreach (var header in headers)
                    {
                        TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                        cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    foreach (var exam in group)
                    {
                        TableRow row = new TableRow(new TableRowProperties(new CantSplit()));

                        row.Append(CreateColoredCell(exam.ExamDate.ToString("dd.MM.yyyy"), bgColor));
                        row.Append(CreateColoredCell(exam.Section?.Name ?? "", bgColor));
                        row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(c => c.Degrees.Name) ?? new List<string>()), bgColor));
                        row.Append(CreateColoredCell(string.Join(", ", exam.ExamCommissions?
                                                        .Select(c => $"{c.Commission.CommissionNo} - {c.Commission.Name}")
                                                        ?? new List<string>()), bgColor));
                        row.Append(CreateColoredCell(string.Join(", ", exam.ExamSubjects?.Select(c => c.Subjects.Name) ?? new List<string>()), bgColor));
                        row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                        row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? ""}, {exam.ExamBuilding?.Address ?? ""}", bgColor));

                        table.Append(row);
                    }
                    body.Append(table);
                }


                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }

                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);

                var paragraph = new Paragraph();
                paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                paragraph.Append(new Run(
                    new RunProperties(new NoProof()),
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                paragraph.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                ));
                paragraph.Append(new Run(new Text("1"))); // Placeholder
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                Footer footer = new Footer(paragraph);
                footerPart.Footer = footer;
                footerPart.Footer.Save();

                var sectionProps = new SectionProperties(
                                            new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId },
                                            new PageSize() { Width = 11906U, Height = 16838U, Orient = PageOrientationValues.Portrait },
                                            new PageMargin() { Top = 720, Right = 720U, Bottom = 720, Left = 720U }
                                        );

                body.Append(sectionProps);
                Paragraph bottomNote = new Paragraph(
    new ParagraphProperties(
        new SpacingBetweenLines() { Before = "200" }
    ),
    new Run(
        new RunProperties(new Bold(), new FontSize() { Val = "22" }),
        new Text("Qeyd: Qrafik mütəmadi olaraq yenilənir və digər imtahanların tarixləri də müəyyən olunduqca cədvələ əlavə ediləcək.") { Space = SpaceProcessingModeValues.Preserve }
    )
);
                body.Append(bottomNote);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
        public async Task<MemoryStream> ExportExamScheduleToWordForLetter(int? year)
        {
            var query = _context.Exams
                        .Include(e => e.ExamDegrees)
                            .ThenInclude(d => d.Degrees)
                        .Include(e => e.ExamCommissions)
                            .ThenInclude(c => c.Commission)
                        .Include(e => e.ExamExpertSubProfessions)
                            .ThenInclude(s => s.SubProfession)
                        .Include(e => e.ExamBuilding)
                        .Include(e => e.District)
                        .Include(e => e.Section)
                        .Include(e => e.ExamSubjects)
                            .ThenInclude(e => e.Subjects)
                        .Where(e => e.Type == 1);

            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(e => e.ExamDate.Year == year.Value);
            }

            var exams = await query.OrderBy(e => e.ExamDate)
                                   .ThenBy(e => e.SectionId)
                                   .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);


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

                TableRow headerRow = new TableRow(new TableRowProperties(
                                                      new TableHeader()
                                                  ));
                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Təhsil səviyyəsi", "İmtahan keçirilən şəhər(rayon)", "İmtahan mərkəzinin adı və ünvanı", "İştirakçı Sayı", "Buraxılışın başlanması", "İmtahan başlanması", "İmtahanın bitməsi", "Qeyd" };
                foreach (var header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var exam in exams)
                {
                    TableRow row = new TableRow(
                                            new TableRowProperties(
                                                new CantSplit()
                                            )
                                        );

                    var sectionId = _context.Exams.Where(e => e.Id == exam.Id).Select(e => e.SectionId).FirstOrDefault();
                    string bgColor = "ffffff";



                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );

                    row.Append(CreateColoredCell(exam.ExamDate.ToString("dd.MM.yyyy"), bgColor));
                    row.Append(CreateColoredCell(exam.Section?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(c => c.Degrees.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? ""}, {exam.ExamBuilding?.Address ?? ""}", bgColor));
                    row.Append(CreateColoredCell(exam.StudentCount?.ToString() ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.AdmissionTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.StartTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell(exam.EndTime?.ToString(@"hh\:mm") ?? "", bgColor));
                    row.Append(CreateColoredCell("", bgColor));

                    table.Append(row);
                }

                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }
                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);

                var paragraph = new Paragraph();
                paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                paragraph.Append(new Run(
                    new RunProperties(new NoProof()),
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                paragraph.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                ));
                paragraph.Append(new Run(new Text("1")));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                Footer footer = new Footer(paragraph);
                footerPart.Footer = footer;
                footerPart.Footer.Save();

                var sectionProps = new SectionProperties(
                    new PageSize() { Width = 11906, Height = 16838, Orient = PageOrientationValues.Portrait },
                    new PageMargin() { Top = 720, Right = 720, Bottom = 720, Left = 720 },
                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId }
                );

                body.Append(sectionProps);
                body.Append(table);
                Paragraph bottomNote = new Paragraph(
    new ParagraphProperties(
        new SpacingBetweenLines() { Before = "200" }
    ),
    new Run(
        new RunProperties(new FontSize() { Val = "22" }),
        new Text("Qeyd: Qrafik mütəmadi olaraq yenilənir və digər imtahanların tarixləri də müəyyən olunduqca cədvələ əlavə ediləcək.") { Space = SpaceProcessingModeValues.Preserve }
    )
);
                body.Append(bottomNote);
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
        public async Task<List<Monitor>> GetAvailableVolunteersAsync(int? sectionId)
        {
            return await _context.Monitors.OrderBy(m => m.Surname).Where(m => m.Role == 4 && (m.SectionId == null || m.SectionId != 1)).ToListAsync();
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
                .Where(dr => dr.Type == 2 && dr.Archive == 0)
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

        public async Task AssignVolunteersToExamAsync(int examId, List<int> selectedVolunteerIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("İmtahan tapılmadı.");
            }

            var selectedVolunteers = await _context.Monitors
                .Where(r => selectedVolunteerIds.Contains(r.Id))
                .Where(dr => dr.Role == 4)
                .ToListAsync();

            if (selectedVolunteers.Count != selectedVolunteerIds.Count)
            {
                throw new ArgumentException("Seçilmiş sayıda könüllü yoxdur.");
            }

            foreach (var vol in selectedVolunteers)
            {
                exam.Monitors.Add(vol);
            }

            await _context.SaveChangesAsync();
        }

        public Task<List<Monitor>> GetAvailableWorkersAsync(int buildingId)
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
                throw new Exception($"İmtahan tapılmadı : {examId}");
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

                    body.AppendChild(CreateBoldCenteredParagraph("İŞTİRAK EDƏN NƏZARƏTÇİLƏRİN QEYDİYYAT VƏRƏQİ"));
                    string logoPath = "wwwroot/img/State_Examination_Center_logo.svg.png";
                    AddImageToDocument(mainPart, logoPath);

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

                    Paragraph buildingParagraph = new Paragraph(
                        new ParagraphProperties(new Justification { Val = JustificationValues.Center }), new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }),
                            new Text("Qabiliyyət imtahanının keçirildiyi imtahan binası:")),

                        new Run(new RunProperties(new Bold(),
                                                new FontSize { Val = "28" }, new Underline { Val = UnderlineValues.Single }),
                            new Text(exam.ExamBuilding?.Name?.ToString()))
                    );
                    body.AppendChild(buildingParagraph);

                    body.AppendChild(CreateBoldCenteredParagraph("______________________________________________________________________"));

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

                    TableCell cellVezifesi = new TableCell();
                    cellVezifesi.AppendChild(new Paragraph(new Run(new Text("Vəzifəsi (imtahan zalı, məşq zalı)"))));
                    cellVezifesi.TableCellProperties = new TableCellProperties();
                    cellVezifesi.TableCellProperties.AppendChild(new GridSpan() { Val = 2 });
                    cellVezifesi.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellVezifesi);

                    TableCell cellName = new TableCell();
                    cellName.AppendChild(new Paragraph(new Run(new Text("Soyadı, adı, ata adı"))));
                    cellName.TableCellProperties = new TableCellProperties();
                    cellName.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellName.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellName);

                    TableCell cellImza1 = new TableCell();
                    cellImza1.AppendChild(new Paragraph(new Run(new Text("İmza / I növbə"))));
                    cellImza1.TableCellProperties = new TableCellProperties();
                    cellImza1.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Restart });
                    cellImza1.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow1.AppendChild(cellImza1);

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

                    TableCell cellInnovbe = new TableCell();
                    cellInnovbe.AppendChild(new Paragraph(new Run(new Text("I növbə"))));
                    cellInnovbe.TableCellProperties = new TableCellProperties();
                    cellInnovbe.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow2.AppendChild(cellInnovbe);

                    TableCell cellIInnovbe = new TableCell();
                    cellIInnovbe.AppendChild(new Paragraph(new Run(new Text("II növbə"))));
                    cellIInnovbe.TableCellProperties = new TableCellProperties();
                    cellIInnovbe.TableCellProperties.AppendChild(new Shading { Fill = "D9D9D9" });
                    headerRow2.AppendChild(cellIInnovbe);

                    TableCell cellNameContinue = new TableCell();
                    cellNameContinue.AppendChild(new Paragraph());
                    cellNameContinue.TableCellProperties = new TableCellProperties();
                    cellNameContinue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellNameContinue);

                    TableCell cellImza1Continue = new TableCell();
                    cellImza1Continue.AppendChild(new Paragraph());
                    cellImza1Continue.TableCellProperties = new TableCellProperties();
                    cellImza1Continue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellImza1Continue);

                    TableCell cellImza2Continue = new TableCell();
                    cellImza2Continue.AppendChild(new Paragraph());
                    cellImza2Continue.TableCellProperties = new TableCellProperties();
                    cellImza2Continue.TableCellProperties.AppendChild(new VerticalMerge() { Val = MergedCellValues.Continue });
                    headerRow2.AppendChild(cellImza2Continue);

                    table.AppendChild(headerRow2);

                    var monitorCount = monitors.Count;

                    for (int i = 0; i < monitorCount; i++)
                    {
                        TableRow dataRow = new TableRow();

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

            string relationshipId = mainPart.GetIdOfPart(imagePart);

            var element =
                new Drawing(
                    new wp.Inline(
                        new wp.Extent { Cx = 990000L, Cy = 792000L },
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
        public async Task<IEnumerable<Exam>> GetExamsForExportAsync()
        {
            return await _context.Exams
                .Include(e => e.ExamBuilding)
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.Id)
                .ToListAsync();
        }
        public async Task<List<Exam>> GetBySectionBuildingAndYearAsync(
    int? sectionId,
    int? examBuildingId,
    int? year)
        {
            IQueryable<Exam> query = _context.Exams
                .Include(e => e.ExamBuilding)
                .Include(e => e.Section)
                .Include(e => e.ExamCommissions)
                    .ThenInclude(ec => ec.Commission);

            if (sectionId.HasValue)
                query = query.Where(e => e.SectionId == sectionId);

            if (examBuildingId.HasValue)
                query = query.Where(e => e.ExamBuldingId == examBuildingId);

            if (year.HasValue)
                query = query.Where(e => e.ExamDate.Year == year);

            return await query
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();
        }
        public async Task<MemoryStream> ExportExamCalendar(int? year)
        {
            var query = _context.Exams
                        .Include(e => e.ExamDegrees)
                            .ThenInclude(d => d.Degrees)
                        .Include(e => e.ExamCommissions)
                            .ThenInclude(c => c.Commission)
                        .Include(e => e.ExamExpertSubProfessions)
                            .ThenInclude(s => s.SubProfession)
                        .Include(e => e.ExamBuilding)
                        .Include(e => e.District)
                        .Include(e => e.Section)
                        .Include(e => e.ExamSubjects)
                            .ThenInclude(e => e.Subjects)
                        .Where(e => e.Type == 1);

            if (year.HasValue && year.Value != 0)
            {
                query = query.Where(e => e.ExamDate.Year == year.Value);
            }

            var exams = await query.OrderBy(e => e.ExamDate)
                                   .ThenBy(e => e.SectionId)
                                   .ToListAsync();

            MemoryStream memoryStream = new MemoryStream();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                mainPart.Document.Append(body);

                Paragraph topTitle = new Paragraph(
    new ParagraphProperties(
        new Justification() { Val = JustificationValues.Center },
        new SpacingBetweenLines() { After = "200" }
    ),
    new Run(
        new RunProperties(new Bold(), new FontSize() { Val = "28" }),
        new Text("Qabiliyyət imtahanlarının 11 iyun 2026-cı il tarixinə olan qrafiki haqqında") { Space = SpaceProcessingModeValues.Preserve },
        new Break(),
        new Text("Məlumat")
    )
);
                body.Append(topTitle);
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

                TableRow headerRow = new TableRow(new TableRowProperties(
                                                      new TableHeader()
                                                  ));
                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Təhsil səviyyəsi", "Komissiya", "İmtahan fənləri", "İmtahan keçirilən şəhər(rayon)", "İmtahan mərkəzinin adı və ünvanı" };
                foreach (var header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(header))));
                    cell.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(cell);
                }
                table.Append(headerRow);

                foreach (var exam in exams)
                {
                    TableRow row = new TableRow(
                                            new TableRowProperties(
                                                new CantSplit()
                                            )
                                        );

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
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(c => c.Degrees.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamCommissions?
                                                        .Select(c => $"{c.Commission.CommissionNo} - {c.Commission.Name}")
                                                        ?? new List<string>()),
                                                    bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamSubjects?.Select(c => c.Subjects.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "", bgColor));
                    row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? ""}, {exam.ExamBuilding?.Address ?? ""}", bgColor));

                    table.Append(row);
                }
                TableCell CreateColoredCell(string text, string bgColor)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(text))));
                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor }
                    );
                    cell.Append(cellProperties);
                    return cell;
                }

                FooterPart footerPart = mainPart.AddNewPart<FooterPart>();
                string footerPartId = mainPart.GetIdOfPart(footerPart);

                var paragraph = new Paragraph();
                paragraph.Append(new ParagraphProperties(new Justification() { Val = JustificationValues.Center }));
                paragraph.Append(new Run(
                    new RunProperties(new NoProof()),
                    new FieldChar() { FieldCharType = FieldCharValues.Begin }
                ));
                paragraph.Append(new Run(
                    new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }
                ));
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.Separate }
                ));
                paragraph.Append(new Run(new Text("1"))); // Placeholder
                paragraph.Append(new Run(
                    new FieldChar() { FieldCharType = FieldCharValues.End }
                ));

                Footer footer = new Footer(paragraph);
                footerPart.Footer = footer;
                footerPart.Footer.Save();

                var sectionProps = new SectionProperties(
                    new PageSize() { Width = 11906, Height = 16838, Orient = PageOrientationValues.Portrait },
                    new PageMargin() { Top = 720, Right = 720, Bottom = 720, Left = 720 },
                    new FooterReference() { Type = HeaderFooterValues.Default, Id = footerPartId }
                );

                body.Append(sectionProps);
                body.Append(table);
                Paragraph bottomNote = new Paragraph(
    new ParagraphProperties(
        new SpacingBetweenLines() { Before = "200" }
    ),
    new Run(
        new RunProperties(new Bold(), new FontSize() { Val = "22" }),
        new Text("Qeyd: Qrafik mütəmadi olaraq yenilənir və digər imtahanların tarixləri də müəyyən olunduqca cədvələ əlavə ediləcək.") { Space = SpaceProcessingModeValues.Preserve }
    )
);
                body.Append(bottomNote);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return memoryStream;

        }

        public class SeedVerifyResult
        {
            public bool Found { get; set; }
            public bool SeedMatches { get; set; }
            public bool Reproducible { get; set; }     // snapshot + seed → orijinal nəticə ilə eyni
            public bool MatchesCurrent { get; set; }   // hazırda təyin olunanlarla eyni
            public List<int> RecomputedIds { get; set; } = new();
            public List<int> OriginalIds { get; set; } = new();
            public DateTime? AssignedAt { get; set; }
            public string? Message { get; set; }
        }

        public async Task<SeedVerifyResult> VerifyAssignmentAsync(int examId, byte assignmentType, int[] seed)
        {
            var log = await _context.AssignmentSeedLogs
                .Where(l => l.ExamId == examId && l.AssignmentType == assignmentType)
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            if (log == null)
                return new SeedVerifyResult { Found = false, Message = "Bu imtahan üçün seed qeydi tapılmadı." };

            var seedMatches = log.Seed1 == seed[0] && log.Seed2 == seed[1]
                           && log.Seed3 == seed[2];

            var pool = SeededSelector.ParsePool(log.CandidatePool);
            var recomputed = SeededSelector
                .Order(pool, seed, p => p.Id, p => p.Count)
                .Take(log.NumberRequested)
                .Select(p => p.Id)
                .ToList();

            var original = SeededSelector.ParseIds(log.SelectedIds);

            // Hazırda təyin olunanlar
            List<int> current = assignmentType switch
            {
                1 => await _context.ExamExperts.Where(x => x.ExamId == examId).Select(x => x.ExpertId).ToListAsync(),
                2 or 3 => await _context.ExamMonitors.Where(x => x.ExamId == examId).Select(x => x.MonitorId).ToListAsync(),
                _ => new List<int>()
            };

            return new SeedVerifyResult
            {
                Found = true,
                SeedMatches = seedMatches,
                Reproducible = seedMatches && recomputed.SequenceEqual(original),
                MatchesCurrent = original.OrderBy(x => x).SequenceEqual(current.OrderBy(x => x)),
                RecomputedIds = recomputed,
                OriginalIds = original,
                AssignedAt = log.CreatedAt,
                Message = seedMatches ? "Seed uyğun gəldi." : "Daxil edilən seed orijinaldan fərqlidir."
            };
        }

        // old expert assignment
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

            var availableExperts = await _context.Experts
                                    .Where(e => e.SectionId == exam.SectionId &&
                                                e.ExpertsProfessions.Any(sp => selectedSubProfessions.Contains(sp.SubProfessionId)) &&
                                                !assignedExpertIds.Contains(e.Id) &&
                                                e.Archive == 0 &&
                                                e.Status == 0 &&
                                                e.Federation == federationId &&
                                                !_context.ExamExpertSubProfessions
                                         .Any(ees => ees.ExamId == examId && ees.ExpertId == e.Id)
                                     && !_context.ExamExpertSubProfessions
                                         .Any(ees => ees.ExpertId == e.Id && ees.Exam.ExamDate == exam.ExamDate))
                                    .Include(e => e.Exams)
                                    .Include(e => e.ExamExpertSubProfessions)
                                        .ThenInclude(ees => ees.Exam)
                                    .AsSplitQuery()                       // ← cartesian explosion-u aradan qaldırır
                                    .ToListAsync();

            if (availableExperts.Count < numberOfExperts)
            {
                throw new InvalidOperationException("Yetərli sayda ekspert yoxdur.");
            }

            var selectedExperts = availableExperts.GroupBy(e => e.ThisYearAssignmentCount)
                                                  .OrderBy(g => g.Key)
                                                  .SelectMany(g => g.OrderBy(_ => Guid.NewGuid()))
                                                  .Take(numberOfExperts)
                                                  .ToList();
            var shuffledSubProfessions = subProfessions.OrderBy(x => Guid.NewGuid()).ToList();

            // ── N+1 sorğularını döngüdən çıxarırıq (DB döngü boyunca dəyişmir) ──
            var selectedExpertIds = selectedExperts.Select(e => e.Id).ToList();

            // 1) Eyni imtahan tarixində artıq təyin olunmuş ekspertlər (tək sorğu)
            var assignedOnSameDateSet = (await _context.ExamExpertSubProfessions
                    .Where(ees => selectedExpertIds.Contains(ees.ExpertId) && ees.Exam.ExamDate == exam.ExamDate)
                    .Select(ees => ees.ExpertId)
                    .Distinct()
                    .ToListAsync())
                .ToHashSet();

            // 2) Bu imtahan üçün DB-də mövcud təyinatlar (tək sorğu)
            var existingAssignmentSet = (await _context.ExamExpertSubProfessions
                    .Where(ees => ees.ExamId == examId)
                    .Select(ees => new { ees.ExpertId, ees.SubProfessionId, ees.FederationId, ees.RoomId })
                    .ToListAsync())
                .Select(a => (a.ExpertId, (int?)a.SubProfessionId, (int?)a.FederationId, a.RoomId))
                .ToHashSet();
            // ───────────────────────────────────────────────────────────────────

            for (int i = 0; i < selectedExperts.Count; i++)
            {
                var expert = selectedExperts[i];

                var isAssignedToAnotherExam = assignedOnSameDateSet.Contains(expert.Id);

                if (isAssignedToAnotherExam)
                {
                    continue;
                }

                exam.Experts.Add(expert);

                var assignedSubProfession = shuffledSubProfessions[i % shuffledSubProfessions.Count];

                bool existsInDatabase = existingAssignmentSet.Contains(
                    (expert.Id, (int?)assignedSubProfession.Id, (int?)federationId, roomId));

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
    }
}