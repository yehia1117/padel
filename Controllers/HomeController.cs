using Microsoft.AspNetCore.Mvc;
namespace PadelBooking.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
