using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "NaturaTypeNavigation" };
            var query = await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
            return await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);
        }

        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "NaturaTypeNavigation" };
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
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "WorkerTypeNavigation", "ExamBuilding", "ExamMonitors.Exams", "ExamMonitors.ExamRooms", "MonitorLogs", "Contracts", "NaturaTypeNavigation" };
            var natura = await _naturaRepository.GetByIdAsync(id, null, includes); 

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
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "NaturaTypeNavigation" };

            var monitors = await _naturaRepository.GetAllAsync(sectionId, 3, null, includes);

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

            var dt = new DataTable("Naturaçılar");
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
                new DataColumn("SSN"),
                new DataColumn("Rekvizit"),
                new DataColumn("Hesablaşma hesabı"),
                new DataColumn("VÖEN"),
                new DataColumn("Bank filialı"),
                new DataColumn("Bank filial kodu"),
                new DataColumn("İstiqamət"),
                new DataColumn("Rayon"),
                new DataColumn("Növ"),
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
                    monitor.SSN,
                    monitor.Rekvizit,
                    monitor.HesablashmaH,
                    monitor.Voen,
                    monitor.BankFilial,
                    monitor.BankFilialCode,
                    monitor.Section?.Name,
                    monitor.DistrictNavigation?.Name,
                    monitor.NaturaTypeNavigation?.Name,
                    monitor.AssignmentCount
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
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "NaturaTypeNavigation" };
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
        public async Task<IEnumerable<Monitor>> GetMonitorLogsAsync(int? sectionId)
        {
            return await _naturaRepository.GetMonitorLogsAsync(sectionId);
        }
        public async Task<IEnumerable<Monitor>> GetMonitorLogsBySupervisorIdAsync(int monitorId)
        {
            return await _naturaRepository.GetMonitorLogsBySupervisorIdAsync(monitorId);
        }
        public async Task<byte[]> ExportContractsToWordAsync(List<int> selectedMonitorIds, DateTime contractDate)
        {
            // 1) Monitorları ve mevcut Contract’ları al
            var monitors = await _context.Monitors
                .Include(m => m.Contracts)
                .Where(m => selectedMonitorIds.Contains(m.Id))
                .Where(m => m.Archive == 0 && m.Role == 3 && m.Status == 0)
                .ToListAsync();

            // 2) Yeni Contract nesnelerini oluştur
            var newContracts = new List<Contract>();
            foreach (var monitor in monitors)
            {
                int nextNumber = monitor.Contracts.Count + 1;
                string formattedNumber = nextNumber.ToString("D2");
                string contractNo = $"QNT{monitor.FinCode}-{formattedNumber}";

                newContracts.Add(new Contract
                {
                    Number = contractNo,
                    Date = contractDate,
                    MonitorId = monitor.Id
                });
            }

            // 3) DB’ye kaydet
            if (newContracts.Any())
            {
                await _context.Contracts.AddRangeAsync(newContracts);
                await _context.SaveChangesAsync();
            }

            // 4) Şablonu oku
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates",
                                            "Naturaci-müqavile.docx");
            byte[] templateBytes = await File.ReadAllBytesAsync(templatePath);
            using var templateStream = new MemoryStream(templateBytes);
            using var templateDoc = WordprocessingDocument.Open(templateStream, false);

            // Şablon body’sini çek
            var templateBody = templateDoc.MainDocumentPart.Document.Body;
            var templateElements = templateBody.Elements<OpenXmlElement>().ToList();

            // 5) Yeni belgeyi oluştur ve gerekli part’ları ekle
            using var output = new MemoryStream();
            using (var newDoc = WordprocessingDocument.Create(output, WordprocessingDocumentType.Document))
            {
                var mainPart = newDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body;

                // Şablondaki stilleri/numbering’i/theme’i/font tablosunu kopyala
                if (templateDoc.MainDocumentPart.StyleDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.StyleDefinitionsPart);
                if (templateDoc.MainDocumentPart.NumberingDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.NumberingDefinitionsPart);
                if (templateDoc.MainDocumentPart.ThemePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.ThemePart);
                if (templateDoc.MainDocumentPart.FontTablePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.FontTablePart);

                // 6) Her bir contract için şablon elementlerini kopyala ve placeholder’ları değiştir
                foreach (var contract in newContracts)
                {
                    var monitor = monitors.First(m => m.Id == contract.MonitorId);
                    var fullName = $"{monitor.Surname} {monitor.Name} {monitor.Fname}";

                    // Sayfa kırılımı
                    if (body.HasChildren)
                        body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

                    foreach (var elem in templateElements)
                    {
                        var clone = elem.CloneNode(true);

                        if (clone is Table table)
                        {
                            var rows = table.Elements<TableRow>().ToList();
                            if (rows.Any(r => r.InnerText.Contains("İcraçı")))
                            {
                                // Placeholder : Data mapping for İcraçı table
                                var placeholders = new Dictionary<string, string>
                                        {
                                            { "Soyadı, adı, atasının adı", fullName },
                                            { "Şəxsiyyət vəsiqəsinin FİN kodu", monitor.FinCode ?? "" },
                                            { "Sosial sığorta nömrəsi", monitor.SSN ?? "" },
                                            { "VÖEN (olduğu təqdirdə)", monitor.Voen ?? "" },
                                            { "Bankın Adı", monitor.BankFilial ?? "" },
                                            { "Bankın Kodu", monitor.BankFilialCode ?? "" },
                                            { "Hesablaşma hesabı", monitor.HesablashmaH ?? "" },
                                            { "Hesab nömrəsi", monitor.Rekvizit ?? "" }
                                        };

                                foreach (var row in rows)
                                {
                                    var texts = row.Descendants<Text>().ToList();
                                    var combinedText = string.Join("", texts.Select(t => t.Text));

                                    foreach (var placeholder in placeholders)
                                    {
                                        if (combinedText.Contains(placeholder.Key))
                                        {
                                            // Tüm Text elemanlarını sil
                                            foreach (var t in texts)
                                                t.Text = "";

                                            // Yeni değeri ilk run’a ekle (veya yeni run yarat)
                                            var firstRun = row.Descendants<Run>().FirstOrDefault();
                                            if (firstRun != null)
                                            {
                                                var newRun = new Run(new Text($"{placeholder.Key}: {placeholder.Value}"));
                                                var runProps = new RunProperties(
                                                    new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial" },
                                                    new FontSize { Val = "24" }
                                                );
                                                newRun.PrependChild(runProps);
                                                firstRun.Parent.InsertAfter(newRun, firstRun);
                                            }

                                            break; // Aynı satıra birden fazla yerleştirme yapma
                                        }
                                    }
                                }
                            }
                            body.AppendChild(table);
                            continue;
                        }

                        if (clone is Paragraph p)
                        {
                            var text = p.InnerText.Trim();

                            if (text.Contains("MÜQAVİLƏ №"))
                            {
                                if (p.ParagraphProperties == null)
                                    p.ParagraphProperties = new ParagraphProperties();
                                p.ParagraphProperties.Append(new Justification { Val = JustificationValues.Center });

                                foreach (var run in p.Elements<Run>().ToList())
                                {
                                    if (run.RunProperties == null)
                                        run.RunProperties = new RunProperties();
                                    if (!run.RunProperties.Elements<Bold>().Any())
                                        run.RunProperties.Append(new Bold());
                                    var rf = run.RunProperties.Elements<RunFonts>().FirstOrDefault()
                                             ?? run.RunProperties.AppendChild(new RunFonts());
                                    rf.Ascii = rf.HighAnsi = rf.EastAsia = "Arial";
                                }

                                var nr = new Run(new Text($" {contract.Number}"));
                                nr.RunProperties = new RunProperties(new Bold(),
                                    new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial" });
                                p.AppendChild(nr);
                            }
                            // 2) "Tarix:" satırına tarih ekle
                            else if (text.Contains("Bakı şəhəri"))
                            {
                                var ilRun = p.Elements<Run>().FirstOrDefault(r => r.InnerText.Trim() == "Tarix:");
                                if (ilRun != null)
                                    ilRun.AppendChild(new Text($" {contractDate:dd.MM.yyyy}"));
                                else
                                    p.Elements<Run>().Last().AppendChild(new Text($" {contractDate:dd.MM.yyyy}"));
                            }
                            // 3) Alt çizgi placeholder yerine fullname
                            else if (p.InnerText.Contains("_"))
                            {
                                foreach (var txt in p.Descendants<Text>())
                                {
                                    if (txt.Text.Contains("_"))
                                        txt.Text = fullName;
                                }
                            }
                        }

                        body.AppendChild(clone);
                    }
                }

                mainPart.Document.Save();
            }

            return output.ToArray();
        }
    }
}
