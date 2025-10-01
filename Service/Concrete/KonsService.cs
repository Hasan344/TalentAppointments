using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using ForQab.Repository.Abstract;
using ForQab.Repository.Concrete;
using ForQab.Service.Abstract;
using System.Drawing;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Service
{
    public class KonsService : IKonsService
    {
        private readonly IKonsRepository _konsRepository;
        private readonly MyDbContext _context;

        public KonsService(IKonsRepository konsRepository, MyDbContext context)
        {
            _konsRepository = konsRepository;
            _context = context;
        }

        public async Task AddAsync(KonsViewModel entity)
        {
            await _konsRepository.AddAsync(entity);
        }

        //public async Task BulkAddAsync(IEnumerable<Expert> monitors)
        //{
        //    await _konsRepository.BulkAddAsync(monitors);
        //}

        public async Task DeleteAsync(int id)
        {
            await _konsRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId)
        {
            return await _konsRepository.GetAllAsync(sectionId);
        }
        public Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
        {
            return _konsRepository.GetSubProfessionsAsync(sectionId);
        }
        public async Task<IEnumerable<Expert>> GetAllAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
        {
            //var includes = new string[] { "DistrictNavigation", "SubProfessions", "Section", "GenderNavigation" };

            var query = await _konsRepository.GetAllAsync(sectionId);
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
            if (subProfessionId.HasValue && subProfessionId > 0)
            {
                query = query.Where(m => m.ExpertsProfessions.Any(sp => sp.SubProfessionId == subProfessionId.Value)).ToList();
            }
            return query;
        }

        public async Task<Expert> GetByIdAsync(int id)
        {
            var expert = await _konsRepository.GetByIdAsync(id);
            if (expert == null)
                throw new Exception($"{id} Id-yə sahib expert yoxdur.");

            string photoPath = $@"\\teshkilat-db\Images\Talent\{expert.FinCode}.jpg";
            expert.Photo = ConvertToBase64(photoPath);
            return expert;
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
            return await _konsRepository.GetSectionsAsync(sectionId);
        }

        public async Task UpdateAsync(KonsEditViewModel entity)
        {
            await _konsRepository.UpdateAsync(entity);
        }
        public async Task UpdateAsync(Expert entity)
        {
            await _konsRepository.UpdateAsync(entity);
        }
        public async Task BulkAddAsync(IEnumerable<Expert> experts)
        {
            await _konsRepository.BulkAddAsync(experts);
        }

        public async Task<IEnumerable<Expert>> GetKonsLogsAsync(int? sectionId)
        {
            return await _konsRepository.GetKonsLogsAsync(sectionId);
        }
        public async Task<byte[]> ExportContractsToWordAsync(List<int> selectedKonsIds, DateTime contractDate)
        {
            
            var kons = await _context.Experts
                                     .Include(e => e.Contracts)
                                     .Where(m => selectedKonsIds.Contains(m.Id))
                                     .Where(m => m.Archive == 0 && m.Kons == true && m.Status == 0)
                                     .ToListAsync();

            
            var newContracts = new List<Contract>();
            foreach (var kon in kons)
            {
                int nextNumber = kon.Contracts.Count + 1;
                string formattedNumber = nextNumber.ToString("D2");
                string contractNo = $"XQK{kon.FinCode}-{formattedNumber}";

                newContracts.Add(new Contract
                {
                    Number = contractNo,
                    Date = contractDate,
                    ExpertId = kon.Id
                });
            }

            if (newContracts.Any())
            {
                await _context.Contracts.AddRangeAsync(newContracts);
                await _context.SaveChangesAsync();
            }

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates",
                                            "konsertmeyster müqavilə.docx");
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
                    var kon = kons.First(m => m.Id == contract.ExpertId);
                    var fullName = $"{kon.Surname} {kon.Name} {kon.Fname}";

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
                                            { "Şəxsiyyət vəsiqəsinin FİN kodu", kon.FinCode ?? "" },
                                            { "Sosial sığorta nömrəsi", kon.SSN ?? "" },
                                            { "VÖEN (olduğu təqdirdə)", kon.Voen ?? "" },
                                            { "Bankın Adı", kon.BankFilial ?? "" },
                                            { "Bankın Kodu", kon.BankFilialCode ?? "" },
                                            { "Hesablaşma hesabı", kon.HesablashmaH ?? "" },
                                            { "Hesab nömrəsi", kon.Rekvizit ?? "" }
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
    }
}
