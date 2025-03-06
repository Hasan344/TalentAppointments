using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore;
using ForQab.Repository.Abstract;
using Microsoft.EntityFrameworkCore.Storage;

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
                        FederationId = federationId // Yeni eklendi
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
                .Include(e => e.Monitors)
                .Include(e => e.ExamDegrees)
                    .ThenInclude(ed => ed.Degrees)
                .Include(e => e.District)
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId)
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
            existingExam.DistrictId = exam.DistrictId;

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

        public async Task AssignWorkersToExamAsync(int examId, List<int> selectedWorkerIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("Exam not found.");
            }

            var selectedWorkers = await _context.Monitors
                .Where(r => selectedWorkerIds.Contains(r.Id))
                .ToListAsync();

            if (selectedWorkers.Count != selectedWorkerIds.Count)
            {
                throw new ArgumentException("One or more selected representatives not found.");
            }

            foreach (var rep in selectedWorkers)
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
                string[] headers = { "İmtahan Tarixi", "İstiqamət", "Təhsil Səviyyəsi", "Komissiya", "İxtisas", "İmtahan keçirilən rayon", "İmtahan mərkəzinin adı", "İştirakçı Sayı", "Buraxılışın başlanması", "İmtahan başlanması", "İmtahanın bitməsi" };
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
                        bgColor = "f4bc72";
                    }
                    else if (sectionId == 2)
                    {
                        bgColor = "50bdda";
                    }
                    else if (sectionId == 3)
                    {
                        bgColor = "8b1c00";
                    }



                    TableCellProperties cellProperties = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = bgColor } // Arka plan rengi
                    );

                    row.Append(CreateColoredCell(exam.ExamDate.ToString("dd.MM.yyyy"), bgColor));
                    row.Append(CreateColoredCell(exam.Section?.Name ?? "N/A", bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamDegrees?.Select(d => d.Degrees.Name) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamCommissions?.Select(c => c.Commission.CommissionNo) ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(string.Join(", ", exam.ExamExpertSubProfessions?.Select(s => s.SubProfession.Name).Distinct() ?? new List<string>()), bgColor));
                    row.Append(CreateColoredCell(exam.District?.Name ?? "N/A", bgColor));
                    row.Append(CreateColoredCell($"{exam.ExamBuilding?.Name ?? "N/A"}, {exam.ExamBuilding?.Address ?? "N/A"}", bgColor));
                    row.Append(CreateColoredCell(exam.StudentCount?.ToString() ?? "N/A", bgColor));
                    row.Append(CreateColoredCell(exam.AdmissionTime?.ToString(@"hh\:mm") ?? "N/A", bgColor));
                    row.Append(CreateColoredCell(exam.StartTime?.ToString(@"hh\:mm") ?? "N/A", bgColor));
                    row.Append(CreateColoredCell(exam.EndTime?.ToString(@"hh\:mm") ?? "N/A", bgColor));

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
            return await _context.DimRepresentatives.OrderBy(dr => dr.Surname).ToListAsync();
        }

        public async Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("Exam not found.");
            }

            var selectedRepresentatives = await _context.DimRepresentatives
                .Where(r => selectedRepresentativeIds.Contains(r.Id))
                .ToListAsync();

            if (selectedRepresentatives.Count != selectedRepresentativeIds.Count)
            {
                throw new ArgumentException("One or more selected representatives not found.");
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
        .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new Exception($"Exam not found for ID: {examId}");  // 🔥 Log ekleyerek kontrol et
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
    }
}
