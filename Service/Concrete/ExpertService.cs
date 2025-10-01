using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Threading;

namespace ForQab.Service;

public class ExpertService : IExpertService
{
    private readonly IExpertRepository _expertRepository;
    private readonly MyDbContext _context;

    public ExpertService(IExpertRepository expertRepository, MyDbContext context)
    {
        _expertRepository = expertRepository;
        _context = context;
    }

    public async Task<IEnumerable<Expert>> GetAllExpertsAsync()
    {
        var experts = await _expertRepository.GetAllAsync();
        if (experts == null)
            throw new Exception("Expert tapılmadı.");
        return experts;
    }

    public async Task<Expert?> GetExpertByIdAsync(int id)
    {
        var expert = await _expertRepository.GetByIdAsync(id);
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

    public async Task AddExpertAsync(ExpertViewModel expertViewModel)
    {
        if (string.IsNullOrWhiteSpace(expertViewModel.Name))
            throw new ArgumentException("Ad boş ola bilməz.");

        await _expertRepository.AddAsync(expertViewModel);
    }

    public async Task UpdateExpertAsync(ExpertEditViewModel expert)
    {
        if (expert.Id <= 0)
            throw new ArgumentException("Id Xətası.");

        var existingExpert = await _expertRepository.GetByIdAsync(expert.Id);
        if (existingExpert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.UpdateAsync(expert);
    }

    public async Task DeleteExpertAsync(int id)
    {
        var expert = await _expertRepository.GetByIdAsync(id);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.DeleteAsync(id);
    }

    public async Task AddSubProfessionToExpertAsync(int expertId, SubProfession subProfession)
    {
        if (string.IsNullOrWhiteSpace(subProfession.Name))
            throw new ArgumentException("SubProfession name cannot be empty.");

        var expert = await _expertRepository.GetByIdAsync(expertId);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.AddSubProfessionToExpertAsync(expertId, subProfession);
    }

    public async Task RemoveSubProfessionFromExpertAsync(int expertId, int subProfessionId)
    {
        var expert = await _expertRepository.GetByIdAsync(expertId);
        if (expert == null)
            throw new Exception($"{expert.Id} Id-yə sahib expert yoxdur.");

        await _expertRepository.RemoveSubProfessionFromExpertAsync(expertId, subProfessionId);
    }

    public async Task<IEnumerable<Expert>> SearchExpertsByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Search term cannot be empty.");

        return await _expertRepository.SearchByNameAsync(name);
    }
    public async Task<List<Section>> GetSectionsAsync(int? sectionId)
    {
        return await _expertRepository.GetSectionsAsync(sectionId);  
    }
    public async Task<List<Profession>> GetFederationsAsync(int? sectionId)
    {
        return await _expertRepository.GetFederationsAsync(sectionId);
    }

    public Task<IEnumerable<SubProfession>> GetSubProfessionsAsync(int? sectionId)
    {
        return _expertRepository.GetSubProfessionsAsync(sectionId);
    }
    public async Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions.SubProfession", "Section", "GenderNavigation", "FederationNavigation", "Contracts", "ExamExpertSubProfessions", "ExamExpertSubProfessions.Exam" };
        return await _expertRepository.GetAllAsync(sectionId,null,includes);
    }
    public async Task<IEnumerable<Expert>> GetExpertsBySectionIdAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? federationId, int? subProfessionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions.SubProfession", "Section", "GenderNavigation", "FederationNavigation", "Contracts", "ExamExpertSubProfessions" };

        var query = await _expertRepository.GetAllAsync(sectionId, null, includes);

        query = query.OrderBy(q => q.Surname).ThenBy(q => q.Name).ToList();
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
            query = query.Where(m => m.District.HasValue && m.District == district.Value).ToList();
        }
        if (startYear.HasValue)
            query = query.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year >= startYear.Value).ToList();

        if (endYear.HasValue)
            query = query.Where(m => m.BirthDate.HasValue && m.BirthDate.Value.Year <= endYear.Value).ToList();

        if (federationId.HasValue)
        {
            query = query.Where(f => f.Federation.HasValue && f.Federation ==  federationId).ToList();
        }

        if (subProfessionId.HasValue && subProfessionId > 0)
        {
            query = query.Where(m => m.ExpertsProfessions.Any(sp => sp.SubProfessionId == subProfessionId.Value)).ToList();
        }

        return query;
    }
    public async Task<IEnumerable<Expert>> GetArchivedExpertsBySectionIdAsync(int? sectionId, string? searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
    {
        var includes = new string[] { "DistrictNavigation", "ExpertsProfessions", "Section", "GenderNavigation", "FederationNavigation", "Contracts", "ExamExpertSubProfessions" };

        var query = await _expertRepository.GetAllArchivedAsync(sectionId, null, includes);
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
            query = query.Where(m => m.BirthDate.Value.Year >= startYear.Value).ToList(); 
        if (endYear.HasValue)
            query = query.Where(m => m.BirthDate.Value.Year <= endYear.Value).ToList();

        if (subProfessionId.HasValue && subProfessionId > 0)
        {
            query = query.Where(m => m.ExpertsProfessions.Any(sp => sp.SubProfessionId == subProfessionId.Value)).ToList();
        }

        return query;
    }
    public async Task BulkAddAsync(IEnumerable<Expert> experts)
    {
        await _expertRepository.BulkAddAsync(experts);
    }

    public async Task<IEnumerable<Expert>> GetExpertLogsAsync(int? sectionId)
    {
        return await _expertRepository.GetExpertLogsAsync(sectionId);
    }

    public async Task<IEnumerable<Expert>> GetExpertLogsByExpertIdAsync(int expertId)
    {
        return await _expertRepository.GetExpertLogsByExpertIdAsync(expertId);
    }

    public async Task DeleteExpertLogs(int? id)
    {
        await _expertRepository.DeleteExpertLogs(id);
    }

    public async Task UpdateExpertAsync(Expert expert)
    {
        await _expertRepository.UpdateExpertAsync(expert);
    }
    public async Task<List<SubProfession>> GetSubProfessionsByFederationAsync(int federationId)
    {
        return await _expertRepository.GetSubProfessionsByFederationAsync(federationId);
    }
    public async Task<byte[]> ExportContractsToWordAsync(List<int> selectedExpertIds, DateTime contractDate)
    {
        var experts = await _context.Experts.Include(m => m.Contracts)
            .Where(m => selectedExpertIds.Contains(m.Id))
            .Where(m => m.Archive == 0 && m.Kons == false && m.Status == 0)
            .ToListAsync();

        var newContracts = new List<Contract>();
        foreach (var expert in experts)
        {
            int nextNumber = expert.Contracts.Count + 1;
            string formattedNumber = nextNumber.ToString("D2");
            string contractNo = $"XQE{expert.FinCode}-{formattedNumber}";

            newContracts.Add(new Contract
            {
                Number = contractNo,
                Date = contractDate,
                ExpertId = expert.Id
            });
        }

        if (newContracts.Any())
        {
            await _context.Contracts.AddRangeAsync(newContracts);
            await _context.SaveChangesAsync();
        }

        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates",
                                        "Muqavile Ekspert QABİLİYYET 2024son.docx");
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
                var expert = experts.First(m => m.Id == contract.ExpertId);
                var fullName = $"{expert.Surname} {expert.Name} {expert.Fname}";

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
                                            { "Şəxsiyyət vəsiqəsinin FİN kodu", expert.FinCode ?? "" },
                                            { "Sosial sığorta nömrəsi", expert.SSN ?? "" },
                                            { "VÖEN (olduğu təqdirdə)", expert.Voen ?? "" },
                                            { "Bankın Adı", expert.BankFilial ?? "" },
                                            { "Bankın Kodu", expert.BankFilialCode ?? "" },
                                            { "Hesablaşma hesabı", expert.HesablashmaH ?? "" },
                                            { "Hesab nömrəsi", expert.Rekvizit ?? "" }
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
