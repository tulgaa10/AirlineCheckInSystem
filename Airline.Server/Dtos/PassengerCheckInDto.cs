public class PassengerCheckInDto
{
    public string PassportNumber { get; set; } = string.Empty;
    public int FlightId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
}
