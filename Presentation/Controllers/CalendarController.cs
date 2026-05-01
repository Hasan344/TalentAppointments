using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class CalendarController : Controller
{
    private readonly MyDbContext _context;

    public CalendarController(MyDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetCalendarEvents()
    {
        var exams = await _context.Exams
            .Include(e => e.Section)
            .Include(e => e.ExamBuilding)
            .Select(e => new
            {
                id = e.Id,
                title = e.Name,
                start = e.ExamDate.ToDateTime(TimeOnly.MinValue)
                                       .ToString("yyyy-MM-dd"),
                // Extended props — panel üçün
                extendedProps = new
                {
                    sectionId = e.SectionId,
                    section = e.Section != null ? e.Section.Name : "",
                    building = e.ExamBuilding != null ? e.ExamBuilding.Name : "",
                    examDate = e.ExamDate.ToString("dd.MM.yyyy"),
                    startTime = e.StartTime.HasValue
                                     ? e.StartTime.Value.ToString(@"hh\:mm")
                                     : "",
                    endTime = e.EndTime.HasValue
                                     ? e.EndTime.Value.ToString(@"hh\:mm")
                                     : "",
                    studentCount = e.StudentCount
                }
            })
            .ToListAsync();

        return Json(exams);
    }
}