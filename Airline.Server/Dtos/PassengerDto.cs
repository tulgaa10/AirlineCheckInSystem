/// <summary>
/// PassengerDto angilal ni aylagcin medeelliig damjuulahad zoriulsan DTO (Data Transfer Object).
/// Ene ni UI deer haruulah zoriulalttai buh niislegchiin medeelliig aguulna.
/// </summary>
public class PassengerDto
{
    /// <summary>
    /// aylagcin ogogdliin sand burtgegdsen dawtahgui ID dugaar
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// aylagcin buren ner
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// aylagcin passport dugaar, system dotor heregleh tusgai taniltiin kod
    /// </summary>
    public string PassportNumber { get; set; } = string.Empty;

    /// <summary>
    /// aylagcin yamar niislegt burtgegdsen tuhai niislegiin ID
    /// </summary>
    public int FlightId { get; set; }

    /// <summary>
    /// aylagcin dugaar - zovhon harulahiin tuld aguulagdaj bolno (null baina)
    /// </summary>
    public string? FlightNumber { get; set; }

    /// <summary>
    /// aylagcin songoj avsan suudliin dugaar - onoogogdoogui baina (null baina)
    /// </summary>
    public string? SeatNumber { get; set; }
}