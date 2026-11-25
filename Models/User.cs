namespace PadelBooking.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<Booking> Bookings { get; set; } = new();
}
