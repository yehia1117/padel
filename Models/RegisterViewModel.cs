using System.ComponentModel.DataAnnotations;
namespace PadelBooking.Models;
public class RegisterViewModel
{
    [Required, StringLength(60, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, RegularExpression("^01[0-2,5][0-9]{8}$", ErrorMessage = "اكتب رقم موبايل مصري صحيح (11 رقم)."), StringLength(11)] public string Phone { get; set; } = "";
    [Required, MinLength(6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل.")] public string Password { get; set; } = "";
}
