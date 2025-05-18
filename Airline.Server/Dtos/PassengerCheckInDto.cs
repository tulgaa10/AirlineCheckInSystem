using System.ComponentModel.DataAnnotations;

/// <summary>
/// PassengerCheckInDto angilal ni niislegchdiin check-in hiihed ashiglagdah DTO.
/// Niislegchiin check-in-ii uildeld shaardlagatai buh medeelliig aguulna.
/// </summary>
public class PassengerCheckInDto
{
    /// <summary>
    /// Niislegchiin passport dugaar - zaavval baih shaardlagatai field
    /// </summary>
    [Required]
    public string PassportNumber { get; set; } = string.Empty;

    /// <summary>
    /// Niislegiin ID - niislegchiin yamar niislegt check-in hiij baigaag todorhoilno
    /// </summary>
    [Required]
    public int FlightId { get; set; }

    /// <summary>
    /// Niislegchiin songoj baigaa suudliin dugaar - zaavval baih shaardlagatai field
    /// </summary>
    [Required]
    public string SeatNumber { get; set; } = string.Empty;
}