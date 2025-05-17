using System.ComponentModel.DataAnnotations;

public class PassengerCheckInDto
{
    [Required]
    public string PassportNumber { get; set; } = string.Empty;
    [Required]

    public int FlightId { get; set; }
    [Required]
    public string SeatNumber { get; set; } = string.Empty;
}
