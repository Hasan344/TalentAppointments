using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CalendarController : Controller
{
    private readonly MyDbContext _context;

    public CalendarController(MyDbContext context)
    {
        _context = context;
    }



    [HttpGet]
    public IActionResult GetCalendarEvents()
    {
        var exams = _context.Exams
            .Include(e=>e.Section)
     .Select(e => new
     {
         id=e.Id,
         section=e.Section.Name,
         title = $"{e.Name} - {e.Section.Name}", // Sınav adı
         start = e.ExamDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-ddTHH:mm:ss") // DateOnly -> DateTime
     })
     .ToList();


        return Json(exams);
    }
    public IActionResult Index()
    {
        return View();
    }
}