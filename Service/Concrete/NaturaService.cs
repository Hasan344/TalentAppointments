using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Repository.Concrete;
using ForQab.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Globalization;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Service
{

    public class NaturaService : INaturaService
    {
        private readonly INaturaRepository _naturaRepository;
        private readonly MyDbContext _context;

        public NaturaService(INaturaRepository naturaRepository, MyDbContext context)
        {
            _naturaRepository = naturaRepository;
            _context = context;
        }

        public async Task AddAsync(Monitor entity)
        {
            await _naturaRepository.AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _naturaRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
            return await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
        }

        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
            if (genderId.HasValue)
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
            var natura = await _naturaRepository.GetByIdAsync(id); 

            string photoPath = $@"\\teshkilat-db\Images\Talent\{natura.FinCode}.jpg";
            natura.Photo = ConvertToBase64(photoPath);

            return natura;
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
            return await _naturaRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(Monitor entity)
        {
            await _naturaRepository.UpdateAsync(entity);
        }
        public async Task BulkAddAsync(IEnumerable<Monitor> monitors)
        {
            await _naturaRepository.BulkAddAsync(monitors);
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
                        string genderName = row.Cell(5).GetValue<string>();
                        string districtName = row.Cell(2).GetString();
                        string sectionName = row.Cell(6).GetString();
                        byte? genderId = genders.FirstOrDefault(g => g.Name == genderName)?.Id;
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
                            Role = 3,
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

                    await _naturaRepository.BulkAddAsync(monitors);
                }
            }

            return "Naturalar uğurla idxal edildi.";
        }
        public async Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };

            var monitors = await _naturaRepository.GetAllAsync(sectionId, 2, null, includes);

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
                    return stream.ToArray(); // Byte dizisi olarak döndürüyoruz
                }
            }
        }

        public async Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section" };
            var query = await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
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
            return await _naturaRepository.GetMonitorLogsAsync();
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _naturaRepository.GetMonitorLogsBySupervisorIdAsync(monitorId);
        }
    }
}
