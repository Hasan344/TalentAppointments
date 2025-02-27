using ClosedXML.Excel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Repository;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Service
{
    public class HeadMonitorService : IHeadMonitorService
    {
        private readonly IHeadMonitorRepository _headMonitorRepository;
        private readonly MyDbContext _context;

        public HeadMonitorService(IHeadMonitorRepository headMonitorRepository, MyDbContext context)
        {
            _headMonitorRepository = headMonitorRepository;
            _context = context;
        }

        public async Task AddAsync(Monitor entity)
        {
            await _headMonitorRepository.AddAsync(entity);
        }

        public async Task BulkAddAsync(IEnumerable<Monitor> monitors)
        {
            await _headMonitorRepository.BulkAddAsync(monitors);
        }

        public async Task DeleteAsync(int id)
        {
            await _headMonitorRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
            return await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
        }
        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId,string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };

            var query = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
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

            return query.Where(h => h.Archive==0).ToList();
        }

        public async Task<Monitor> GetByIdAsync(int id)
        {
            return await _headMonitorRepository.GetByIdAsync(id);
        }

        public async Task<List<Section>> GetSectionsAsync(int? sectionId)
        {
            return await _headMonitorRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(Monitor entity)
        {
            await _headMonitorRepository.UpdateAsync(entity);
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
                        string genderName = row.Cell(5).GetValue<string>();
                        string districtName = row.Cell(9).GetString();
                        string sectionName = row.Cell(4).GetString();
                        byte? genderId = genders.FirstOrDefault(g => g.Name == genderName)?.Id;
                        int? districtId = districts.FirstOrDefault(d => d.Name == districtName)?.Id;
                        int? sectionId = sections.FirstOrDefault(s => s.Name == sectionName)?.Id;

                        var monitor = new Monitor
                        {
                            Name = row.Cell(1).GetString(),
                            Surname = row.Cell(2).GetString(),
                            Fname = row.Cell(3).GetString(),
                            Archive = 0,
                            Gender = genderId,
                            Role = 1,
                            VNum = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetValue<string?>(),
                            Profession = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetString(),
                            Workplace = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString(),
                            Position = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetString(),
                            BirthDate = row.Cell(10).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(10).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            TelEv = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetString(),
                            TelIs = row.Cell(12).IsEmpty() ? null : row.Cell(12).GetString(),
                            FinCode = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetString(),
                            Serial = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            SectionId = sectionId,
                            District = districtId,
                            SSN = row.Cell(15).GetString(),
                            Rekvizit = row.Cell(16).GetString(),
                            HesablashmaH = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                            Voen = row.Cell(18).IsEmpty() ? null : row.Cell(18).GetString(),
                            BankFilial = row.Cell(19).GetString(),
                            BankFilialCode = row.Cell(20).GetString(),
                        };

                        monitors.Add(monitor);
                    }

                    await _headMonitorRepository.BulkAddAsync(monitors);
                }
            }

            return "HeadMonitor-lər uğurla idxal edildi.";
        }
        public async Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };

            var monitors = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
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
                new DataColumn("Bölmə"),
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
                workbook.Worksheets.Add(dt, "İmtahan rəhbərləri");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray(); // Byte dizisi olarak döndürüyoruz
                }
            }
        }
        public async Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
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

            return query.Where(m => m.Archive == 1).ToList();
        }

        public async Task<IEnumerable<Monitor>> GetMonitorLogsAsync()
        {
            return await _headMonitorRepository.GetMonitorLogsAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _headMonitorRepository.GetMonitorLogsBySupervisorIdAsync(monitorId);
        }

        public async Task DeleteMonitorLogs(int? id)
        {
            await _headMonitorRepository.DeleteMonitorLogs(id);
        }

        public async Task UpdateModelAsync(HeadMonitorEditViewModel model)
        {
            await _headMonitorRepository.UpdateAsync(model);
        }
    }
}
