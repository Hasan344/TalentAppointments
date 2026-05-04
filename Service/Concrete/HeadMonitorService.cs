using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
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

            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "ExamMonitors" };
            var query = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
            return await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);
        }
        public async Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId,string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "ExamMonitors" };

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
            
            if (!string.IsNullOrEmpty(finCode))
            {
                query = query.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (district.HasValue && district > 0)
            {
                query = query.Where(m => m.District == district.Value).ToList();
            }
            if (startYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year >= startYear.Value).ToList(); 
            if (endYear.HasValue)
                query = query.Where(m => m.BirthDate.Value.Year <= endYear.Value).ToList();

            return query.Where(h => h.Archive==0).ToList();
        }

        public async Task<Monitor> GetByIdAsync(int id)
        {
            var includes = new string[]
            {
        "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section",
        "WorkerTypeNavigation", "ExamBuilding",
        "ExamMonitors.Exams", "ExamMonitors.ExamRooms",
        "MonitorLogs", "Contracts", "ExamMonitors"
            };

            var monitor = await _headMonitorRepository.GetByIdAsync(id, null, includes);
            if (monitor == null) return null;

            if (!string.IsNullOrEmpty(monitor.Photo))
            {
                return monitor;
            }

            string photoPath = $@"\\teshkilat-db\Images\Talent\{monitor.FinCode}.jpg";
            var fromShare = ConvertToBase64(photoPath);
            if (!string.IsNullOrEmpty(fromShare))
            {
                monitor.Photo = fromShare;
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

                    foreach (var row in worksheet.RowsUsed().Skip(1)) 
                    {
                        string finCode = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetString();

                        if (!string.IsNullOrEmpty(finCode))
                        {
                            bool exists = await _context.Monitors.AnyAsync(m => m.FinCode == finCode);
                            if (exists)
                            {
                                return $"'{finCode}' FinCode-a sahib istifadəçi artıq mövcuddur. İdxala icazə verilmir.";
                            }
                        }

                        string districtName = row.Cell(2).GetString();
                        string sectionName = row.Cell(6).GetString();
                        int? districtId = districts.FirstOrDefault(d => d.Name == districtName)?.Id;
                        int? sectionId = sections.FirstOrDefault(s => s.Name == sectionName)?.Id;

                        var monitor = new Monitor
                        {
                            VNum = row.Cell(1).IsEmpty() ? null : row.Cell(1).GetValue<string?>(),
                            District = districtId,
                            Surname = row.Cell(3).GetString(),
                            Name = row.Cell(4).GetString(),
                            Fname = row.Cell(5).GetString(),
                            SectionId = sectionId,
                            Archive = 0,
                            Status = 0,
                            AssignmentCount = 0,
                            Gender = row.Cell(7).GetValue<byte>(),
                            Serial = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString(),
                            SerialPrefix = row.Cell(9).IsEmpty() ? null : row.Cell(9).GetString(),
                            TelIs = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                            FinCode = finCode,
                            BirthDate = row.Cell(12).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(12).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            Uni = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetString(),
                            Role = 1,
                            Workplace = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            Position = row.Cell(15).IsEmpty() ? null : row.Cell(15).GetString(),
                            Voen = row.Cell(16).IsEmpty() ? null : row.Cell(16).GetString(),
                            HesablashmaH = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                            Rekvizit = row.Cell(18).GetString(),
                            SSN = row.Cell(19).GetString(),
                            BankFilial = row.Cell(20).GetString(),
                            BankFilialCode = row.Cell(21).GetString(),
                        };

                        monitors.Add(monitor);
                    }

                    await _headMonitorRepository.BulkAddAsync(monitors);
                }
            }

            return "İmtahan rəhbərləri uğurla idxal edildi.";
        }

        public async Task<byte[]> ExportToExcelAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "ExamMonitors" };

            var monitors = await _headMonitorRepository.GetAllAsync(sectionId, 1, null, includes);

            monitors = monitors.Where(m => m.Archive == 0 && m.Status == 0).ToList();

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

            var dt = new DataTable("Rəhbərlər");
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
                new DataColumn("SSN"),
                new DataColumn("Rekvizit"),
                new DataColumn("Hesablaşma hesabı"),
                new DataColumn("VÖEN"),
                new DataColumn("Bank filialı"),
                new DataColumn("Bank filial kodu"),
                new DataColumn("İstiqamət"),
                new DataColumn("Rayon"),
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
                    monitor.SSN,
                    monitor.Rekvizit,
                    monitor.HesablashmaH,
                    monitor.Voen,
                    monitor.BankFilial,
                    monitor.BankFilialCode,
                    monitor.Section?.Name,
                    monitor.DistrictNavigation?.Name,
                    monitor.ComputedAssignmentCount
                );
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "İmtahan rəhbərləri");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray(); 
                }
            }
        }
        public async Task<IEnumerable<Monitor>> GetAllArchivedAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var includes = new string[] { "DistrictNavigation", "RoleNavigation", "GenderNavigation", "Section", "Contracts", "ExamMonitors" };
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
            
            if (!string.IsNullOrEmpty(finCode))
            {
                query = query.Where(m => m.FinCode.Contains(finCode, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(serial))
            {
                query = query.Where(m => m.Serial.Contains(serial, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
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
            return await _headMonitorRepository.GetMonitorLogsAsync(sectionId);
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
        public async Task<byte[]> ExportContractsToWordAsync(List<int> selectedMonitorIds, DateTime contractDate)
        {
            var monitors = await _context.Monitors
                .Include(m => m.Contracts)
                .Where(m => selectedMonitorIds.Contains(m.Id))
                .Where(m => m.Archive == 0 && m.Role == 1 && m.Status == 0)
                .ToListAsync();

            var newContracts = new List<Contract>();
            foreach (var monitor in monitors)
            {
                int nextNumber = monitor.Contracts.Count + 1;
                string formattedNumber = nextNumber.ToString("D2");
                string contractNo = "";
                if (monitor.SectionId == 1)
                {
                     contractNo = $"İR{monitor.FinCode}-{formattedNumber}";
                }
                else
                {
                    contractNo = $"QİR{monitor.FinCode}-{formattedNumber}";

                }

                newContracts.Add(new Contract
                {
                    Number = contractNo,
                    Date = contractDate,
                    MonitorId = monitor.Id
                });
            }

            if (newContracts.Any())
            {
                await _context.Contracts.AddRangeAsync(newContracts);
                await _context.SaveChangesAsync();
            }

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates",
                                            "İmtahan rəhbəri-müqavile.docx");
            byte[] templateBytes = await File.ReadAllBytesAsync(templatePath);
            using var templateStream = new MemoryStream(templateBytes);
            using var templateDoc = WordprocessingDocument.Open(templateStream, false);

            var templateBody = templateDoc.MainDocumentPart.Document.Body;
            var templateElements = templateBody.Elements<OpenXmlElement>().ToList();

            using var output = new MemoryStream();
            using (var newDoc = WordprocessingDocument.Create(output, WordprocessingDocumentType.Document))
            {
                var mainPart = newDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body;

                if (templateDoc.MainDocumentPart.StyleDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.StyleDefinitionsPart);
                if (templateDoc.MainDocumentPart.NumberingDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.NumberingDefinitionsPart);
                if (templateDoc.MainDocumentPart.ThemePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.ThemePart);
                if (templateDoc.MainDocumentPart.FontTablePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.FontTablePart);

                foreach (var contract in newContracts)
                {
                    var monitor = monitors.First(m => m.Id == contract.MonitorId);
                    var fullName = $"{monitor.Surname} {monitor.Name} {monitor.Fname}";

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
                                            foreach (var t in texts)
                                                t.Text = "";

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

                                            break; 
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
                            
                            else if (text.Contains("Bakı şəhəri"))
                            {
                                var ilRun = p.Elements<Run>().FirstOrDefault(r => r.InnerText.Trim() == "Tarix:");
                                if (ilRun != null)
                                    ilRun.AppendChild(new Text($" {contractDate:dd.MM.yyyy}"));
                                else
                                    p.Elements<Run>().Last().AppendChild(new Text($" {contractDate:dd.MM.yyyy}"));
                            }
                            
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
        public async Task<List<int>> FilterSelectedMonitorsAsync(
     List<int> selectedIds, string searchName, int? districtId)
        {
            var query = _context.Monitors.AsQueryable();

            query = query.Where(m => selectedIds.Contains(m.Id));

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var keyword = searchName.Trim().ToLower();
                query = query.Where(m =>
                    (m.Name + " " + m.Surname).ToLower().Contains(keyword) ||
                    (m.Surname + " " + m.Name).ToLower().Contains(keyword));
            }

            if (districtId.HasValue)
                query = query.Where(m => m.District == districtId);

            return await query.Select(m => m.Id).ToListAsync();
        }
        public async Task<byte[]> ExportContractToWordAsync(int monitorId)
        {
            
            var monitor = await _context.Monitors
                .Include(m => m.Contracts)
                .Where(m => m.Id == monitorId && m.Archive == 0 && m.Role == 1 && m.Status == 0)
                .FirstOrDefaultAsync();

            if (monitor == null)
                throw new Exception("Nəzarətçi tapılmadı və ya müqaviləsi yoxdur.");

            var latestContract = monitor.Contracts
            .OrderByDescending(c => c.Id)
            .FirstOrDefault();

            if (latestContract == null)
                throw new Exception("Müqaviləsi yoxdur.");



            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates",
                                            "İmtahan rəhbəri-müqavile.docx");
            byte[] templateBytes = await File.ReadAllBytesAsync(templatePath);
            using var templateStream = new MemoryStream(templateBytes);
            using var templateDoc = WordprocessingDocument.Open(templateStream, false);
            
            var templateBody = templateDoc.MainDocumentPart.Document.Body;
            var templateElements = templateBody.Elements<OpenXmlElement>().ToList();

            using var output = new MemoryStream();
            using (var newDoc = WordprocessingDocument.Create(output, WordprocessingDocumentType.Document))
            {
                var mainPart = newDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body;

                // Copy styles, numbering, theme, and font table
                if (templateDoc.MainDocumentPart.StyleDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.StyleDefinitionsPart);
                if (templateDoc.MainDocumentPart.NumberingDefinitionsPart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.NumberingDefinitionsPart);
                if (templateDoc.MainDocumentPart.ThemePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.ThemePart);
                if (templateDoc.MainDocumentPart.FontTablePart != null)
                    mainPart.AddPart(templateDoc.MainDocumentPart.FontTablePart);

                // Process template elements for the single contract
                var fullName = $"{monitor.Surname} {monitor.Name} {monitor.Fname}";

                foreach (var elem in templateElements)
                {
                    var clone = elem.CloneNode(true);

                    if (clone is Table table)
                    {
                        var rows = table.Elements<TableRow>().ToList();
                        if (rows.Any(r => r.InnerText.Contains("İcraçı")))
                        {
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
                                        foreach (var t in texts)
                                            t.Text = "";
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
                                        break;
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

                            var nr = new Run(new Text($" {latestContract.Number}"));
                            nr.RunProperties = new RunProperties(new Bold(),
                                new RunFonts { Ascii = "Arial", HighAnsi = "Arial", EastAsia = "Arial" });
                            p.AppendChild(nr);
                        }
                        else if (text.Contains("Bakı şəhəri"))
                        {
                            var ilRun = p.Elements<Run>().FirstOrDefault(r => r.InnerText.Trim() == "Tarix:");
                            if (ilRun != null)
                                ilRun.AppendChild(new Text($" {latestContract.Date:dd.MM.yyyy}"));
                            else
                                p.Elements<Run>().Last().AppendChild(new Text($" {latestContract.Date:dd.MM.yyyy}"));
                        }
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

                mainPart.Document.Save();
            }

            return output.ToArray();
        }

        public async Task BulkArchiveAsync(List<int> ids, string archiveReason)
        {
            var monitors = await _context.Monitors
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();

            foreach (var monitor in monitors)
            {
                monitor.Archive = 1;
                monitor.ArchiveReason = archiveReason;
                //expert.Photo = null;
            }

            await _context.SaveChangesAsync();
        }
    }
}