namespace ForQab.Service.Concrete
{
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using ForQab.DataAccess.Models;
    using ForQab.Service.Abstract;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public class BadgeExportService : IBadgeExportService
    {
        private readonly MyDbContext _context;

        public BadgeExportService(MyDbContext context)
        {
            _context = context;
        }

        public byte[] GenerateBadges(int examId)
        {
            var exam = _context.Exams
                .Include(e => e.ExamBuilding)
                .Include(e => e.ExamMonitors).ThenInclude(em => em.Monitors)
                .Include(e => e.ExamMonitors).ThenInclude(em => em.ExamRooms)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eep => eep.Expert)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eep => eep.ExamRoom)
                .Include(e => e.ExamRepresentatives).ThenInclude(eep => eep.Representative)
                .FirstOrDefault(e => e.Id == examId);

            if (exam == null) throw new Exception("İmtahan tapılmadı");

            var badges = new List<BadgeModel>();

            var monitors = exam.ExamMonitors
                .Where(em => em.Monitors.Role == 1 || em.Monitors.Role == 2)
                .Select(em => em.Monitors)
                .Distinct()
                .ToList();

            foreach (var em in monitors)
            {
                var fullName = $"{em.Surname}   {em.Name} {em.Fname}".Trim();
                var roleName = GetMonitorRoleName((int)em.Role);
                string photoPath = $@"\\teshkilat-db\Images\Talent\{em.FinCode}.jpg";
                byte[]? photoBytes = File.Exists(photoPath) ? File.ReadAllBytes(photoPath) : null;

                var examMonitor = exam.ExamMonitors.FirstOrDefault(x => x.MonitorId == em.Id);
                var roomName = examMonitor?.ExamRooms?.Name;

                badges.Add(new BadgeModel
                {
                    FullName = fullName,
                    Role = roleName,
                    Building = exam.ExamBuilding.Name,
                    Room = roomName,
                    Date = exam.ExamDate.ToString("d MMMM yyyy", new CultureInfo("az-Latn-AZ")),
                    PhotoBytes = photoBytes,
                    SectionId = exam.SectionId
                });
            }

            foreach (var eep in exam.ExamExpertSubProfessions)
            {
                var expert = eep.Expert;
                var fullName = $"{expert.Surname} {expert.Name} {expert.Fname}".Trim();
                var roleName = (bool)expert.Kons ? "Konsertmeyster" : "Ekspert";
                string photoPath = $@"\\teshkilat-db\Images\Talent\{expert.FinCode}.jpg";
                byte[]? photoBytes = File.Exists(photoPath) ? File.ReadAllBytes(photoPath) : null;

                badges.Add(new BadgeModel
                {
                    FullName = fullName,
                    Role = roleName,
                    Building = exam.ExamBuilding?.Name,
                    Room = eep.ExamRoom?.Name,
                    Date = exam.ExamDate.ToString("d MMMM yyyy", new CultureInfo("az-Latn-AZ")),
                    PhotoBytes = photoBytes,
                    SectionId = exam.SectionId
                });
            }

            foreach (var eep in exam.ExamRepresentatives)
            {
                var representative = eep.Representative;
                var fullName = $"{representative.Surname} {representative.Name} {representative.Fname}".Trim();
                var roleName = representative.Type == 1 ? "DİM nümayəndəsi" : "Nazirlik nümayəndəsi";
                string photoPath = $@"\\teshkilat-db\Images\Talent\{representative.FinCode}.jpg";
                byte[]? photoBytes = File.Exists(photoPath) ? File.ReadAllBytes(photoPath) : null;

                badges.Add(new BadgeModel
                {
                    FullName = fullName,
                    Role = roleName,
                    Building = exam.ExamBuilding?.Name,
                    Room = "",
                    Date = exam.ExamDate.ToString("d MMMM yyyy", new CultureInfo("az-Latn-AZ")),
                    PhotoBytes = photoBytes,
                    SectionId = exam.SectionId
                });
            }

            return CreateWordFile(badges);
        }

        private string GetMonitorRoleName(int role)
        {
            return role switch
            {
                1 => "İmtahan rəhbəri",
                2 => "Nəzarətçi",
                _ => "Naməlum"
            };
        }

        private byte[] CreateWordFile(List<BadgeModel> badges)
        {
            using var mem = new MemoryStream();

            using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();
                mainPart.Document.AppendChild(body);

                int badgesPerRow = 2; // Keep 2 badges per row (2 columns)
                var table = new Table();
                var logoBytes = LoadLogoBytes();

                table.AppendChild(new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.None },
                        new BottomBorder { Val = BorderValues.None },
                        new LeftBorder { Val = BorderValues.None },
                        new RightBorder { Val = BorderValues.None },
                        new InsideHorizontalBorder { Val = BorderValues.None },
                        new InsideVerticalBorder { Val = BorderValues.None }
                    )
                ));

                for (int i = 0; i < badges.Count; i += badgesPerRow)
                {
                    var row = new TableRow();

                    for (int j = 0; j < badgesPerRow; j++)
                    {
                        var cell = new TableCell();

                        if (i + j < badges.Count)
                        {
                            var badge = badges[i + j];
                            var badgeTable = CreateBadgeTable(mainPart, badge, logoBytes);
                            cell.Append(badgeTable);

                            var cellProps = new TableCellProperties(
                                new TableCellBorders(
                                    new TopBorder { Val = BorderValues.Single, Size = 4, Color = "D9D9D9" },
                                    new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "D9D9D9" },
                                    new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "D9D9D9" },
                                    new RightBorder { Val = BorderValues.Single, Size = 4, Color = "D9D9D9" }
                                ),
                                new TableCellMarginDefault(
                                    new TopMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                                    new BottomMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                                    new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                                    new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }
                                )
                            );

                            cell.PrependChild(cellProps);
                        }

                        row.Append(cell);
                    }

                    table.Append(row);

                    // Add page break after every 3 rows (i.e., 6 badges: 3 rows * 2 badges per row)
                    if ((i / badgesPerRow + 1) % 3 == 0 && (i + badgesPerRow) < badges.Count)
                    {
                        body.Append(table);
                        body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                        table = new Table();
                    }
                }
                var sectionProps = new SectionProperties(
                                        new PageMargin
                                        {
                                            Top = 720, // 1 inch
                                            Bottom = 720,
                                            Left = 720,
                                            Right = 720
                                        }
                                    );
                body.Append(sectionProps);
                body.Append(table);
                mainPart.Document.Save();
            }

            return mem.ToArray();
        }

        private Run CreateRun(string text, bool bold = false, bool italic = false, int fontSize = 11)
        {
            return new Run(
                new RunProperties(
                    new RunFonts { Ascii = "Calibri" },
                    new FontSize { Val = (fontSize * 2).ToString() },
                    bold ? new Bold() : null,
                    italic ? new Italic() : null
                ),
                new Text(text) { Space = SpaceProcessingModeValues.Preserve }
            );
        }


        private class BadgeModel
        {
            public string FullName { get; set; }
            public string Role { get; set; }
            public string Building { get; set; }
            public string Room { get; set; }
            public string Date { get; set; }
            public int? SectionId { get; set; }
            public byte[]? PhotoBytes { get; set; }
        }

        private byte[] LoadLogoBytes()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "State_Examination_Center_logo.svg.png");
            return File.ReadAllBytes(path);
        }

        private Run AddImageToParagraph(MainDocumentPart mainPart, byte[] imageBytes, string imageName, long cx, long cy, string imageFormat = "png")
        {
            ImagePart imagePart = imageFormat.ToLower() switch
            {
                "jpeg" or "jpg" => mainPart.AddImagePart(ImagePartType.Jpeg),
                _ => mainPart.AddImagePart(ImagePartType.Png),
            };

            using var stream = new MemoryStream(imageBytes);
            imagePart.FeedData(stream);
            var imagePartId = mainPart.GetIdOfPart(imagePart);

            var drawing = new Drawing(
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.Anchor(
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.SimplePosition { X = 100000L, Y = 100000L },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.HorizontalPosition(
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.PositionOffset("0")
                    )
                    {
                        RelativeFrom = DocumentFormat.OpenXml.Drawing.Wordprocessing.HorizontalRelativePositionValues.Column
                    },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.VerticalPosition(
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.PositionOffset("0")
                    )
                    {
                        RelativeFrom = DocumentFormat.OpenXml.Drawing.Wordprocessing.VerticalRelativePositionValues.Paragraph
                    },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent { Cx = cx, Cy = cy },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent
                    {
                        LeftEdge = 0L,
                        TopEdge = 0L,
                        RightEdge = 0L,
                        BottomEdge = 0L
                    },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.WrapNone(),
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties
                    {
                        Id = (UInt32Value)1U,
                        Name = imageName
                    },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                        new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks { NoChangeAspect = true }
                    ),
                    new DocumentFormat.OpenXml.Drawing.Graphic(
                        new DocumentFormat.OpenXml.Drawing.GraphicData(
                            new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties
                                    {
                                        Id = (UInt32Value)0U,
                                        Name = imageName
                                    },
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()
                                ),
                                new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                    new DocumentFormat.OpenXml.Drawing.Blip
                                    {
                                        Embed = imagePartId,
                                        CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print
                                    },
                                    new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())
                                ),
                                new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                    new DocumentFormat.OpenXml.Drawing.Transform2D(
                                        new DocumentFormat.OpenXml.Drawing.Offset { X = 0L, Y = 0L },
                                        new DocumentFormat.OpenXml.Drawing.Extents { Cx = cx, Cy = cy }
                                    ),
                                    new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                        new DocumentFormat.OpenXml.Drawing.AdjustValueList()
                                    )
                                    { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }
                                )
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                )
                {
                    DistanceFromTop = 0U,
                    DistanceFromBottom = 0U,
                    DistanceFromLeft = 0U,
                    DistanceFromRight = 0U,
                    SimplePos = false,
                    RelativeHeight = UInt32Value.FromUInt32(0U),
                    BehindDoc = false,
                    Locked = false,
                    LayoutInCell = true,
                    AllowOverlap = false
                }
            );

            return new Run(drawing);
        }


        private Table CreateBadgeTable(MainDocumentPart mainPart, BadgeModel badge, byte[] logoBytes)
        {
            var innerTable = new Table();

            innerTable.AppendChild(new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.None },
                    new BottomBorder { Val = BorderValues.None },
                    new LeftBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.None },
                    new InsideVerticalBorder { Val = BorderValues.None }
                ),
                new TableWidth { Width = "5500", Type = TableWidthUnitValues.Dxa },
                new TableCellMarginDefault(
                    new TopMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                    new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }
                )
            ));

            var headerRow = new TableRow();

            // Birgə şəkil və logo hüceyrəsi (daxili cədvəl ilə)
            var mediaCell = new TableCell(new TableCellProperties(
                new TableCellWidth { Width = "5000", Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            ));

            var mediaInnerTable = new Table();
            mediaInnerTable.AppendChild(new TableProperties(
                new TableBorders(
                    new TopBorder { Val = BorderValues.None },
                    new BottomBorder { Val = BorderValues.None },
                    new LeftBorder { Val = BorderValues.None },
                    new RightBorder { Val = BorderValues.None },
                    new InsideHorizontalBorder { Val = BorderValues.None },
                    new InsideVerticalBorder { Val = BorderValues.None }
                ),
                new TableWidth { Width = "100%", Type = TableWidthUnitValues.Auto }
            ));

            var mediaRow = new TableRow();

            // Foto hüceyrəsi
            var photoCellInner = new TableCell(new TableCellProperties(
                new TableCellWidth { Width = "2400", Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            ));
            if (badge.PhotoBytes != null)
            {
                var photoRun = AddImageToParagraph(mainPart, badge.PhotoBytes, "Photo.jpg", 1000000L, 1250000L, "jpeg");
                photoCellInner.Append(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Left }),
                    photoRun
                ));
            }
            else
            {
                photoCellInner.Append(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Left }),
                    CreateRun("[Şəkil yoxdur]", false, false, 12)
                ));
            }

            // Logo hüceyrəsi
            var logoCellInner = new TableCell(new TableCellProperties(
                new TableCellWidth { Width = "2600", Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Bottom }
            ));
            var logoRun = AddImageToParagraph(mainPart, logoBytes, "Logo.png", 1266800L, 726800L, "png");
            logoCellInner.Append(new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                logoRun
            ));

            mediaRow.Append(photoCellInner, logoCellInner);
            mediaInnerTable.Append(mediaRow);
            mediaCell.Append(mediaInnerTable);

            headerRow.Append(mediaCell);
            innerTable.Append(headerRow);

            // Content (məlumat) hissəsi
            var contentRow = new TableRow();
            var textCell = new TableCell(new TableCellProperties(
                new TableCellWidth { Width = "5000", Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Bottom }
            ));

            if (badge.Role == "İmtahan rəhbəri" || badge.Role == "DİM nümayəndəsi")
            {
                textCell.Append(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                    new Break(), new Break(), new Break(),
                    CreateRun($"   {badge.Role}", true, false, 24)
                ));

            }
            else if (badge.Role == "Nazirlik nümayəndəsi")
            {
                textCell.Append(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Right }),
                    new Break(), new Break(), new Break(), new Break(), new Break(),
                    CreateRun($"{badge.Role}", true, false, 24)
                ));
            }
            else
            {
                textCell.Append(new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                    new Break(), new Break(), new Break(),
                    CreateRun($"              {badge.Role}", true, false, 24)
                ));
            }




            var roomText = badge.SectionId == 1
                ? $"Məntəqə kodu: {badge.Room}"
                : $"Zalın kodu: {badge.Room}";

            textCell.Append(new Paragraph(
                new ParagraphProperties(new Justification { Val = JustificationValues.Left }),
                CreateRun($"A.S.A.: {badge.FullName}", true, false, 15),
                new Break(),
                CreateRun($"Bina adı: {badge.Building}", true, false, 10),
                new Break(),
                CreateRun(roomText, true, false, 10),
                new Break(),
                CreateRun($"İmtahan tarixi: {badge.Date}", true, false, 10)
            ));

            contentRow.Append(textCell);
            innerTable.Append(contentRow);

            return innerTable;
        }


    }

}
