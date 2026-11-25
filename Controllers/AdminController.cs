using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelBooking.Data;
namespace PadelBooking.Controllers;
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;
    public async Task<IActionResult> Index(string? date)
    {
        DateTime chosen = DateTime.TryParse(date, out var parsed) ? parsed.Date : DateTime.Today;
        var dayStart = chosen.AddHours(16);
        var dayEnd = chosen.AddDays(1); // the 12:00 AM slot belongs to this evening
        var bookings = await _db.Bookings.Include(b => b.User).Where(b => b.StartDateTime >= dayStart && b.StartDateTime <= dayEnd).OrderBy(b => b.StartDateTime).ToListAsync();
        ViewBag.Date = chosen.ToString("yyyy-MM-dd");
        ViewBag.PendingCount = await _db.Bookings.CountAsync(b => b.Status == "Pending");
        ViewBag.TodayCount = await _db.Bookings.CountAsync(b => b.StartDateTime >= DateTime.Today.AddHours(16) && b.StartDateTime <= DateTime.Today.AddDays(1) && b.Status != "Cancelled" && b.Status != "Rejected");
        return View(bookings);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? notes)
    {
        if (status is not ("Confirmed" or "Rejected" or "Cancelled")) return BadRequest();
        var booking = await _db.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        booking.Status = status; booking.AdminNotes = notes;
        await _db.SaveChangesAsync();
        TempData["Message"] = "تم تحديث حالة الحجز.";
        return RedirectToAction("Index", new { date = booking.StartDateTime.ToString("yyyy-MM-dd") });
    }
}
