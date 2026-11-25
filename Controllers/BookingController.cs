using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PadelBooking.Data;
using PadelBooking.Models;
namespace PadelBooking.Controllers;
[Authorize]
public class BookingController : Controller
{
    private readonly AppDbContext _db;
    public BookingController(AppDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index(string? date)
    {
        DateTime chosen;
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out chosen)) chosen = DateTime.Today;
        if (chosen.Date < DateTime.Today) chosen = DateTime.Today;
        var slots = Enumerable.Range(0, 9).Select(i => chosen.Date.AddHours(16 + i)).ToList();
        var booked = await _db.Bookings.Where(b => b.StartDateTime >= chosen.Date.AddHours(16) && b.StartDateTime <= chosen.Date.AddDays(1).AddHours(0) && b.Status != "Rejected" && b.Status != "Cancelled").Select(b => b.StartDateTime).ToListAsync();
        ViewBag.Date = chosen.ToString("yyyy-MM-dd");
        ViewBag.Booked = booked;
        return View(slots);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateTime startDateTime)
    {
        var start = startDateTime;
        var valid = start.Minute == 0 && start.Second == 0 && start >= DateTime.Today.AddHours(16) && start <= DateTime.Today.AddDays(30).AddHours(0);
        // Allowed starts are 4 PM through midnight, with midnight belonging to the selected evening.
        var hourValid = (start.Hour >= 16 && start.Hour <= 23) || start.Hour == 0;
        if (!valid || !hourValid || start < DateTime.Now.AddMinutes(-1)) { TempData["Message"] = "الموعد المختار غير صالح."; return RedirectToAction("Index", new { date = start.Date.ToString("yyyy-MM-dd") }); }
        bool conflict = await _db.Bookings.AnyAsync(b => b.StartDateTime == start && b.Status != "Rejected" && b.Status != "Cancelled");
        if (conflict) { TempData["Message"] = "الموعد تم حجزه بالفعل، اختار موعد تاني."; return RedirectToAction("Index", new { date = start.Hour == 0 ? start.AddDays(-1).Date.ToString("yyyy-MM-dd") : start.Date.ToString("yyyy-MM-dd") }); }
        _db.Bookings.Add(new Booking { UserId = UserId, StartDateTime = start, EndDateTime = start.AddHours(1), Status = "Pending" });
        try { await _db.SaveChangesAsync(); TempData["Message"] = "طلب الحجز اتبعت للإدارة، في انتظار التأكيد."; }
        catch (DbUpdateException) { TempData["Message"] = "الموعد اتاخد للتو، اختار موعد تاني."; }
        return RedirectToAction("MyBookings");
    }

    public async Task<IActionResult> MyBookings() => View(await _db.Bookings.Include(b => b.User).Where(b => b.UserId == UserId).OrderByDescending(b => b.StartDateTime).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == UserId);
        if (booking != null && booking.Status == "Pending" && booking.StartDateTime > DateTime.Now) { booking.Status = "Cancelled"; await _db.SaveChangesAsync(); TempData["Message"] = "تم إلغاء الطلب."; }
        return RedirectToAction("MyBookings");
    }
}
