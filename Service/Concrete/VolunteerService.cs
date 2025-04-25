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
                    var genders = await _context.Genders.ToListAsync();

                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Başlığı atla
                    {
                        string finCode = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString();
                         
                        if (!string.IsNullOrEmpty(finCode))
                        {
                            bool exists = await _context.Monitors.AnyAsync(m => m.FinCode == finCode);
                            if (exists)
                            {
                                return $"'{finCode}' FinCode-a sahib istifadəçi artıq mövcuddur. İdxala icazə verilmir.";
                            }
                        }
                        string genderName = row.Cell(5).GetValue<string>();
                        string districtName = row.Cell(1).GetString();
                        byte? genderId = genders.FirstOrDefault(g => g.Name == genderName)?.Id;
                        int? districtId = districts.FirstOrDefault(d => d.Name == districtName)?.Id;

                        var monitor = new Monitor
                        {
                            Surname = row.Cell(2).GetString(),
                            Name = row.Cell(3).GetString(),
                            Fname = row.Cell(4).GetString(),
                            Archive = 0,
                            Status = 0,
                            AssignmentCount = 0,
                            Gender = row.Cell(5).GetValue<byte>(),
                            Role = 4,
                            BirthDate = row.Cell(9).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(9).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            TelIs = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetString(),
                            FinCode = finCode,
                            Serial = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetString(),
                            SectionId = 1,
                            District = districtId,
                            Uni = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                        };

                        monitors.Add(monitor);
                    }

                    await _volunteerRepository.BulkAddAsync(monitors);
                }
            }

            return "Könüllülər uğurla idxal edildi.";
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
                    return stream.ToArray(); // Byte dizisi olarak döndürüyoruz
                }
            }
        }
    }
}
