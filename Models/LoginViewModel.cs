using System.ComponentModel.DataAnnotations;
namespace PadelBooking.Models;
public class LoginViewModel
{
    [Required] public string Phone { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
