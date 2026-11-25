namespace PadelBooking.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = "Pending";
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
