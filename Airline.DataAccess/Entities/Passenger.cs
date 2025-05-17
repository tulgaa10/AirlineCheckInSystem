using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Passenger
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PassportNumber { get; set; } = string.Empty;

    public int FlightId { get; set; }

    [JsonIgnore]
    public Flight Flight { get; set; } = default!;

    public Seat? Seat { get; set; }
}