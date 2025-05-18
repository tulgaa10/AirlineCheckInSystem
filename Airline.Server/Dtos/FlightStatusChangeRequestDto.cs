/// <summary>
/// FlightStatusChangeRequestDto angilal ni niislegiin statusiig oorchiloh huseltiin medeelliig aguulsan DTO.
/// Ene ni flight manageriin application oor huselt damjuulahad hereglene.
/// </summary>
public class FlightStatusChangeRequestDto
{
    /// <summary>
    /// Yamar niislegiin status oorchilj baigaa tuhai niislegiin ID
    /// </summary>
    public int FlightId { get; set; }

    /// <summary>
    /// Niislegiin shine status - enum utga bolno (Registering, Boarding, Departed, etc.)
    /// </summary>
    public FlightStatus NewStatus { get; set; }
}