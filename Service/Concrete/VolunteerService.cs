using ClosedXML.Excel;
using ForQab.DataAccess.Models;
using ForQab.Migrations;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Globalization;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Service
{
    public class VolunteerService : IVolunteerService
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly MyDbContext _context;

        public VolunteerService(IVolunteerRepository volunteerRepository, MyDbContext context)
        {
            _volunteerRepository = volunteerRepository;
            _context = context;
        }

        public async Task AddAsync(DataAccess.Models.Monitor entity)
        {
            await _volunteerRepository.AddAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _volunteerRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<DataAccess.Models.Monitor>> GetAllAsync(int? sectionId)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section","ExamBuilding" };

            return await _volunteerRepository.GetAllAsync(sectionId, 4, null, includes);
        }

        public async Task<DataAccess.Models.Monitor> GetByIdAsync(int id)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "MonitorLogs", "ExamBuilding" };
            var volunteer = await _volunteerRepository.GetByIdAsync(id, null, includes);

            string photoPath = $@"\\teshkilat-db\Images\Talent\{volunteer.FinCode}.jpg";
            volunteer.Photo = ConvertToBase64(photoPath);

            return volunteer;

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
            return await _volunteerRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(DataAccess.Models.Monitor entity)
        {
            await _volunteerRepository.UpdateAsync(entity);
        }
        public async Task BulkAddAsync(IEnumerable<DataAccess.Models.Monitor> monitors)
        {
            await _volunteerRepository.BulkAddAsync(monitors);
        }
        public async Task<string> ImportFromExcelAsync(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return "XƏTA: Excel faylı yüklənməmişdir.";
            }

            int currentRow = 0; // xəta mesajında satırı göstermek üçün
            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return "XƏTA: Excel faylı düzgün deyil (vərəq tapılmadı).";
                        }

                        var districts = await _context.Districts.ToListAsync();
                        var genders = await _context.Genders.ToListAsync();
                        var sections = await _context.Sections.ToListAsync();

                        var existingFinCodes = (await _context.Monitors
                                .Where(m => m.FinCode != null)
                                .Select(m => m.FinCode)
                                .ToListAsync())
                            .ToHashSet();

                        byte ResolveGender(string raw)
                        {
                            if (string.IsNullOrWhiteSpace(raw)) return 0;
                            raw = raw.Trim();
                            var g = genders.FirstOrDefault(x =>
                                string.Equals(x.Name, raw, StringComparison.OrdinalIgnoreCase));
                            if (g != null) return Convert.ToByte(g.Id);
                            return byte.TryParse(raw, out var b) ? b : (byte)0;
                        }

                        DateOnly? ParseDate(IXLCell cell)
                        {
                            if (cell.IsEmpty()) return null;
                            if (cell.DataType == XLDataType.DateTime)
                                return DateOnly.FromDateTime(cell.GetDateTime());
                            var text = cell.GetString().Trim();
                            if (!DateOnly.TryParseExact(text, "dd/MM/yyyy",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                                throw new FormatException(
                                    $"Doğum tarixi formatı yanlışdır: '{text}'. Düzgün format: gün/ay/il (məs. 05/12/2003).");
                            return d;
                        }

                        var monitors = new List<Monitor>();

                        foreach (var row in worksheet.RowsUsed().Skip(1))
                        {
                            currentRow = row.RowNumber();

                            // Tamamilə boş sətirləri atla
                            if (row.Cell(1).IsEmpty() && row.Cell(2).IsEmpty() && row.Cell(7).IsEmpty())
                                continue;

                            string finCode = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetString().Trim();

                            if (!string.IsNullOrEmpty(finCode) && existingFinCodes.Contains(finCode))
                            {
                                return $"XƏTA: Sətir {currentRow} — '{finCode}' FİN koduna sahib istifadəçi artıq mövcuddur. İdxal dayandırıldı.";
                            }

                            string sectionName = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString().Trim();
                            string districtName = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetString().Trim();

                            int? sectionId = sectionName == null ? 1 : sections.FirstOrDefault(s => s.Name == sectionName)?.Id ?? 1;
                            int? districtId = districtName == null ? null : districts.FirstOrDefault(d => d.Name == districtName)?.Id;

                            var monitor = new Monitor
                            {
                                Name = row.Cell(1).GetString().Trim(),
                                Surname = row.Cell(2).GetString().Trim(),
                                Fname = row.Cell(3).IsEmpty() ? null : row.Cell(3).GetString().Trim(),
                                Gender = ResolveGender(row.Cell(4).GetString()),
                                BirthDate = ParseDate(row.Cell(5)),
                                TelIs = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetString().Trim(),

                                FinCode = finCode,
                                SerialPrefix = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString().Trim(),
                                Serial = row.Cell(9).IsEmpty() ? null : row.Cell(9).GetString().Trim(),

                                SectionId = sectionId,
                                District = districtId,

                                Role = 4,
                                Status = 0,
                                Archive = 0,
                                AssignmentCount = 0,
                            };

                            if (!string.IsNullOrEmpty(finCode))
                                existingFinCodes.Add(finCode);

                            monitors.Add(monitor);
                        }

                        if (monitors.Count == 0)
                            return "XƏTA: Faylda idxal ediləcək sətir tapılmadı (başlıq sətrini silməyin, məlumat 2-ci sətirdən başlamalıdır).";

                        currentRow = 0; // bazaya yazma mərhələsi
                        await _volunteerRepository.BulkAddAsync(monitors);

                        return $"{monitors.Count} könüllü uğurla idxal edildi.";
                    }
                }
            }
            catch (Exception ex)
            {
                var detail = FlattenException(ex);
                return currentRow > 0
                    ? $"XƏTA: Sətir {currentRow} — {detail}"
                    : $"XƏTA: İdxal zamanı problem yarandı — {detail}";
            }
        }

        // Bütün inner exception mesajlarını bir yerə yığır (əsl səbəb çox vaxt inner-də olur)
        private static string FlattenException(Exception ex)
        {
            var messages = new List<string>();
            for (var e = ex; e != null; e = e.InnerException)
                messages.Add(e.Message);
            return string.Join(" → ", messages);
        }
        public async Task<byte[]> ExportToExcelAsync(int? sectionId)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "ExamBuilding" };

            var monitors = await _volunteerRepository.GetAllAsync(sectionId, 4, null, includes);

            var dt = new DataTable("Könüllülər");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Ad"),
                new DataColumn("Soyad"),
                new DataColumn("Ata adı"),
                new DataColumn("Cins"),
                new DataColumn("Rolu"),
                new DataColumn("Vəsiqə nömrəsi"),
                new DataColumn("İş yeri"),
                new DataColumn("Vəzifəsi"),
                new DataColumn("Təvəllüdü"),
                new DataColumn("Telefonu"),
                new DataColumn("FİN kod"),
                new DataColumn("Seriya nömrəsi"),
                new DataColumn("Seriya"),
                new DataColumn("İstiqamət"),
                new DataColumn("Rayon"),
                new DataColumn("İmtahan binası"),
                new DataColumn("İştirak sayı"),
            });

            foreach (var monitor in monitors)
            {
                dt.Rows.Add(
                    monitor.Name,
                    monitor.Surname,
                    monitor.Fname,
                    monitor.GenderNavigation?.Name,
                    monitor.RoleNavigation?.Name,
                    monitor.VNum,
                    monitor.Workplace,
                    monitor.Profession,
                    monitor.BirthDate,
                    monitor.TelIs,
                    monitor.FinCode,
                    monitor.SerialPrefix,
                    monitor.Serial,
                    monitor.Section?.Name,
                    monitor.DistrictNavigation?.Name,
                    monitor.ExamBuilding?.Name,
                    monitor.AssignmentCount
                );
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Könüllülər");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray(); 
                }
            }
        }
    }
}
