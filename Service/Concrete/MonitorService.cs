using ClosedXML.Excel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Service
{
    public class MonitorService : IMonitorService
    {
        private readonly IMonitorRepository _monitorRepository;
        private readonly MyDbContext _context;

        public MonitorService(IMonitorRepository monitorRepository, MyDbContext context)
        {
            _monitorRepository = monitorRepository;
            _context = context;
        }

        public async Task AddAsync(MonitorViewModel entity)
        {
            var monitor = new Monitor
            {
                Name = entity.Name,
                Surname = entity.Surname,
                Fname = entity.Fname,
                Region = entity.Region,
                FinCode = entity.FinCode,
                Serial = entity.Serial,
                SectionId = entity.SectionId,
                Gender = entity.Gender,
                BirthDate = entity.BirthDate,
                ContractNo = entity.ContractNo,
                ContractDate = entity.ContractDate,
                Uni = entity.Uni,
                Position = entity.Position,
                Profession = entity.Profession,
                SSN = entity.SSN,
                Rekvizit = entity.Rekvizit,
                Voen = entity.Voen,
                BankFilial = entity.BankFilial,
                BankFilialCode = entity.BankFilialCode,
                Role = (byte?)entity.Role, 
                Status = (byte?)entity.Status, 
                AssignmentCount = entity.AssignmentCount,
                Archive = (byte)(entity.Archive ?? 0), 
                VNum = entity.VNum,
                Workplace = entity.Workplace,
                HesablashmaH = entity.HesablashmaH,
                TelIs = entity.TelIs,
                District = entity.District,
                ExamMonitors = new List<ExamMonitor>(),
            };
            await _monitorRepository.AddAsync(monitor);

            if (entity.SelectedSubProfessions != null && entity.SelectedSubProfessions.Any())
            {
                var subProfessions = await _context.SubProfessions
                    .Where(sp => entity.SelectedSubProfessions.Contains(sp.Id))
                    .ToListAsync();

                foreach (var subProf in subProfessions)
                {
                    var monitorSubProfession = new MonitorsProfession
                    {
                        MonitorId = monitor.Id,
                        SubProfessionId = subProf.Id
                    };
                    _context.MonitorsProfessions.Add(monitorSubProfession);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _monitorRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "MonitorsProfessions" };
            var query = await _monitorRepository.GetAllAsync(sectionId, 2, null, includes);
            return await _monitorRepository.GetAllAsync(sectionId, 2, null, includes);
        }

        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "MonitorsProfessions" };
            var query = await _monitorRepository.GetAllAsync(sectionId, 2, null, includes);
            if (genderId.HasValue )
            {
                query = query.Where(m => m.Gender == genderId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(m =>
             m.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
             m.Surname.Contains(searchName, StringComparison.OrdinalIgnoreCase))
              .ToList();
            }
            // FinCode filtresi
            if (!string.IsNullOrEmpty(finCode))
            {
                query = query.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Serial filtresi
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // District filtresi
            if (district.HasValue && district > 0)
            {
                query = query.Where(m => m.District == district.Value).ToList();
            }
            if (startYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year >= startYear.Value).ToList(); // Tarixi ilin başlanğıcına çeviririk.
            if (endYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year <= endYear.Value).ToList();

            return query.Where(m => m.Archive == 0).ToList();
        }

        public async Task<Monitor> GetByIdAsync(int id)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "WorkerTypeNavigation", "ExamBuilding", "ExamMonitors.Exams", "ExamMonitors.ExamRooms", "MonitorsProfessions.SubProfession", "MonitorLogs" };

            var monitor = await _monitorRepository.GetByIdAsync(id, null, includes);
            if (monitor != null)
            {
                string photoPath = $@"\\teshkilat-db\Images\Talent\{monitor.FinCode}.jpg";
                monitor.Photo = ConvertToBase64(photoPath);
            }
            return monitor;
        }
        private string ConvertToBase64(string imagePath, int width = 150, int height = 150)
        {
            if (!System.IO.File.Exists(imagePath))
                return null;

            try
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

                using var inputStream = new MemoryStream(imageBytes);
                using var image = Image.FromStream(inputStream);

                using var resized = new Bitmap(image, new Size(width, height));
                using var outputStream = new MemoryStream();
                resized.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                string base64String = Convert.ToBase64String(outputStream.ToArray());
                return $"data:image/jpeg;base64,{base64String}";
            }
            catch
            {
                return null;
            }
        }


        public async Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId)
        {
            return await _monitorRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(Monitor entity)
        {
            await _monitorRepository.UpdateAsync(entity);
        }

        public async Task UpdateAsync(MonitorEditViewModel entity)
        {
            var existingMonitor = await _context.Monitors.Where(m => m.Id == entity.Id)
                .Include(m => m.MonitorsProfessions) 
                .ThenInclude(mp => mp.SubProfession) 
                .FirstOrDefaultAsync();

            if (existingMonitor == null)
            {
                throw new Exception("Monitor tapılmadı");
            }

            // Temel bilgileri güncelle
            existingMonitor.Name = entity.Name;
            existingMonitor.Surname = entity.Surname;
            existingMonitor.Fname = entity.Fname;
            existingMonitor.Region = entity.Region;
            existingMonitor.FinCode = entity.FinCode;
            existingMonitor.Serial = entity.Serial;
            existingMonitor.SectionId = entity.SectionId;
            existingMonitor.Gender = entity.Gender;
            existingMonitor.BirthDate = entity.BirthDate;
            existingMonitor.ContractNo = entity.ContractNo;
            existingMonitor.ContractDate = entity.ContractDate;
            existingMonitor.Uni = entity.Uni;
            existingMonitor.Position = entity.Position;
            existingMonitor.Profession = entity.Profession;
            existingMonitor.SSN = entity.SSN;
            existingMonitor.Rekvizit = entity.Rekvizit;
            existingMonitor.Voen = entity.Voen;
            existingMonitor.BankFilial = entity.BankFilial;
            existingMonitor.BankFilialCode = entity.BankFilialCode;
            existingMonitor.District = entity.District;
            existingMonitor.VNum = entity.VNum;
            existingMonitor.Workplace = entity.Workplace;
            existingMonitor.HesablashmaH = entity.HesablashmaH;
            existingMonitor.TelIs = entity.TelIs;

            if (entity.SelectedSubProfessions != null)
            {
                _context.MonitorsProfessions.RemoveRange(existingMonitor.MonitorsProfessions);
                await _context.SaveChangesAsync(); // Persist the removal first

                foreach (var subProfessionId in entity.SelectedSubProfessions)
                {
                    var subProfession = await _context.SubProfessions.FindAsync(subProfessionId);
                    if (subProfession != null)
                    {
                        // Check if the combination of MonitorId and SubProfessionId already exists
                        var exists = await _context.MonitorsProfessions
                            .AnyAsync(mp => mp.MonitorId == existingMonitor.Id && mp.SubProfessionId == subProfession.Id);

                        if (!exists)
                        {
                            _context.MonitorsProfessions.Add(new MonitorsProfession
                            {
                                MonitorId = existingMonitor.Id,
                                SubProfessionId = subProfession.Id
                            });
                        }
                    }
                }
            }

            _context.Monitors.Update(existingMonitor);
            await _context.SaveChangesAsync();
        }

        public async Task BulkAddAsync(IEnumerable<Monitor> monitors)
        {
            await _monitorRepository.BulkAddAsync(monitors);
        }
        public async Task<string> ImportFromExcelAsync(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return "Excel faylı yüklənməmişdir.";
            }

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return "Excel faylı düzgün deyil.";
                    }

                    var monitors = new List<Monitor>();

                    var districts = await _context.Districts.ToListAsync();
                    var sections = await _context.Sections.ToListAsync();
                    var genders = await _context.Genders.ToListAsync();

                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Başlığı atla
                    {
                        string finCode = row.Cell(12).IsEmpty() ? null : row.Cell(12).GetString();

                        if (!string.IsNullOrEmpty(finCode))
                        {
                            bool exists = await _context.Monitors.AnyAsync(m => m.FinCode == finCode);
                            if (exists)
                            {
                                return $"'{finCode}' FinCode-a sahib istifadəçi artıq mövcuddur. İdxala icazə verilmir.";
                            }
                        }
                        //  string genderName = row.Cell(9).GetValue<string>();
                        string districtName = row.Cell(2).GetString();
                        string sectionName = row.Cell(6).GetString();
                      //  byte? genderId = genders.FirstOrDefault(g => g.Name == genderName)?.Id;
                        int? districtId = districts.FirstOrDefault(d => d.Name == districtName)?.Id;
                        int? sectionId = sections.FirstOrDefault(s => s.Name == sectionName)?.Id;

                        var monitor = new Monitor
                        {
                            
                            Surname = row.Cell(3).GetString(),
                            Name = row.Cell(4).GetString(),
                            Fname = row.Cell(5).GetString(),
                            Archive = 0,
                            Status = 0,
                            AssignmentCount = 0,
                            Gender = row.Cell(9).GetValue<byte>(),
                            Role = 2,
                            VNum = row.Cell(1).IsEmpty() ? null : row.Cell(1).GetValue<string?>(),
                            //Profession = row.Cell(25).IsEmpty() ? null : row.Cell(7).GetString(),
                            Workplace = row.Cell(15).IsEmpty() ? null : row.Cell(8).GetString(),
                            Position = row.Cell(16).IsEmpty() ? null : row.Cell(18).GetString(),
                            BirthDate = row.Cell(13).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(13).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            TelIs = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetString(),
                            FinCode = finCode,
                            Serial = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                            SectionId = sectionId,
                            District = districtId,
                            ContractDate = row.Cell(7).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(7).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            ContractNo = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString(),
                            Uni = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            SSN = row.Cell(20).GetString(),
                            Rekvizit = row.Cell(19).GetString(),
                            HesablashmaH = row.Cell(18).IsEmpty() ? null : row.Cell(18).GetString(),
                            Voen = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                            BankFilial = row.Cell(21).GetString(),
                            BankFilialCode = row.Cell(22).GetString(),
                        };

                        monitors.Add(monitor);
                    }

                    await _monitorRepository.BulkAddAsync(monitors);
                }
            }

            return "Nəzarətçilər uğurla idxal edildi.";
        }
        public async Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "MonitorsProfessions" };

            // Filtreleri içeren veri çekme işlemi
            var monitors = await _monitorRepository.GetAllAsync(sectionId, 2, null, includes);

            if (genderId.HasValue)
                monitors = monitors.Where(m => m.Gender == genderId.Value).ToList();

            if (!string.IsNullOrEmpty(searchName))
                monitors = monitors.Where(m => m.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
                                               m.Surname.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(finCode))
                monitors = monitors.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(serial))
                monitors = monitors.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();

            if (district.HasValue && district > 0)
                monitors = monitors.Where(m => m.District == district.Value).ToList();

            if (startYear.HasValue)
                monitors = monitors.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year >= startYear.Value).ToList();

            if (endYear.HasValue)
                monitors = monitors.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year <= endYear.Value).ToList();

            // Filtrelenmiş veriyi Excel'e aktarma
            var dt = new DataTable("Monitors");
            dt.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Ad"),
        new DataColumn("Soyad"),
        new DataColumn("Ata adı"),
        new DataColumn("Arxiv"),
        new DataColumn("Cins"),
        new DataColumn("Rolu"),
        new DataColumn("Vəzifə nömrəsi"),
        new DataColumn("Peşə"),
        new DataColumn("İş yeri"),
        new DataColumn("Mövqe"),
        new DataColumn("Təvəllüdü"),
        new DataColumn("Ev telefonu"),
        new DataColumn("İş telefonu"),
        new DataColumn("FİN kod"),
        new DataColumn("Seriya"),
        new DataColumn("SSN"),
        new DataColumn("Rekvizit"),
        new DataColumn("Hesablaşma hesabı"),
        new DataColumn("VÖEN"),
        new DataColumn("Bank filialı"),
        new DataColumn("Bank filial kodu"),
        new DataColumn("İstiqamət"),
        new DataColumn("Rayon"),
            });

            foreach (var monitor in monitors)
            {
                dt.Rows.Add(
                    monitor.Name,
                    monitor.Surname,
                    monitor.Fname,
                    monitor.Archive,
                    monitor.GenderNavigation?.Name,
                    monitor.RoleNavigation?.Name,
                    monitor.VNum,
                    monitor.Profession,
                    monitor.Workplace,
                    monitor.Position,
                    monitor.BirthDate,
                    monitor.TelEv,
                    monitor.TelIs,
                    monitor.FinCode,
                    monitor.Serial,
                    monitor.SSN,
                    monitor.Rekvizit,
                    monitor.HesablashmaH,
                    monitor.Voen,
                    monitor.BankFilial,
                    monitor.BankFilialCode,
                    monitor.Section?.Name,
                    monitor.DistrictNavigation?.Name
                );
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Nəzarətçilər");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }


        public async Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "MonitorsProfessions" };
            var query = await _monitorRepository.GetAllAsync(sectionId, 2, null, includes);
            if (genderId.HasValue && genderId > 0)
            {
                query = query.Where(m => m.Gender == genderId.Value).ToList();
            }
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(m =>
             m.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase) ||
             m.Surname.Contains(searchName, StringComparison.OrdinalIgnoreCase))
              .ToList();
            }
            // FinCode filtresi
            if (!string.IsNullOrEmpty(finCode))
            {
                query = query.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Serial filtresi
            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // District filtresi
            if (district.HasValue && district > 0)
            {
                query = query.Where(m => m.District == district.Value).ToList();
            }
            if (startYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year >= startYear.Value).ToList(); // Tarixi ilin başlanğıcına çeviririk.
            if (endYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year <= endYear.Value).ToList();

            return query.Where(m => m.Archive==1).ToList();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsAsync()
        {
            return await _monitorRepository.GetMonitorLogsAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _monitorRepository.GetMonitorLogsBySupervisorIdAsync(monitorId);
        }

        public async Task DeleteMonitorLogs(int? id)
        {
            await _monitorRepository.DeleteMonitorLogs(id);
        }

        public async Task UpdateModelAsync(MonitorEditViewModel model)
        {
            await _monitorRepository.UpdateAsync(model);
        }
        public Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
        {
            return _monitorRepository.GetSubProfessionsAsync(sectionId);
        }
        public async Task<MonitorEditViewModel> GetMonitorForEditAsync(int id)
        {
            var monitor = await _context.Monitors
                .Include(m => m.MonitorsProfessions) 
                    .ThenInclude(mp => mp.SubProfession) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (monitor == null)
            {
                throw new KeyNotFoundException("Monitor not found");
            }

            var sectionId = monitor.SectionId;
            var subProfessions = await GetSubProfessionsAsync(sectionId);

            return new MonitorEditViewModel
            {
                Id = monitor.Id,
                Name = monitor.Name,
                Surname = monitor.Surname,
                Fname = monitor.Fname,
                Region = monitor.Region,
                FinCode = monitor.FinCode,
                Serial = monitor.Serial,
                SectionId = (int)sectionId,
                Gender = monitor.Gender,
                BirthDate = monitor.BirthDate,
                ContractNo = monitor.ContractNo,
                ContractDate = monitor.ContractDate,
                Uni = monitor.Uni,
                Position = monitor.Position,
                Profession = monitor.Profession,
                SSN = monitor.SSN,
                Rekvizit = monitor.Rekvizit,
                Voen = monitor.Voen,
                BankFilial = monitor.BankFilial,
                BankFilialCode = monitor.BankFilialCode,
                District = (byte)monitor.District,
                VNum = monitor.VNum,
                Workplace = monitor.Workplace,
                HesablashmaH = monitor.HesablashmaH,
                TelIs = monitor.TelIs,

                // Monitor'e atanmış SubProfession ID'lerini çekiyoruz
                SelectedSubProfessions = monitor.MonitorsProfessions
                    .Select(mp => mp.SubProfessionId)
                    .ToArray(),

                // Kullanıcıya gösterilecek tüm SubProfessions
                SubProfessions = subProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };
        }

    }
}
