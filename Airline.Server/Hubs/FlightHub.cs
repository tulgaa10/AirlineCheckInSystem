using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// FlightHub angilal ni SignalR Hub-iin ugugdliin damjuulalt.
/// Ene ni niislegiin statusiig real-time ar buh holbogdson client-uud ruu damjuuldag.
/// </summary>
public class FlightHub : Hub
{
    private readonly AirlineDbContext _db;

    /// <summary>
    /// FlightHub angilliin constructor.
    /// DB Context-iig gadnaas damjuulan orulj ogdog.
    /// </summary>
    /// <param name="db">Niislegiin ugugdliin sangiin context</param>
    public FlightHub(AirlineDbContext db) => _db = db;

    /// <summary>
    /// Buh niisleguudiin statusiin medeelliig tataj
    /// buh holbogdson client-uud ruu damjuuldag.
    /// Ene method ni hereglech niislegiin statusiig uurchluh bolgond duudagdana.
    /// </summary>
    /// <returns>Async Task</returns>
    public async Task BroadcastFlightStatuses()
    {
        // Niisleguudiin DTO uusgej, gadaad interface ruu uguh medeelliig bugdiin tataj avna
        var updates = await _db.Flights
            .Select(f => new FlightStatusUpdateDto
            {
                FlightId = f.Id,
                FlightNumber = f.FlightNumber,
                Status = f.Status.ToString()
            })
            .ToListAsync();

        // Buh client ruu "ReceiveFlightUpdates" gedeg method-oor damjuulna
        await Clients.All.SendAsync("ReceiveFlightUpdates", updates);
    }

    /// <summary>
    /// Client holbogdoh uyd duudagdah method.
    /// Shine client holbogdoh bolgond odoo baigaa nislegiin statusiin medeelliig
    /// damjuulj, client-uudiin haruulj baigaa medeelel ni hamgiin shineer haruulna.
    /// </summary>
    /// <returns>Async Task</returns>
    public override async Task OnConnectedAsync()
    {
        // Shine client holbogdmogts niislegiin statusiig damjuulna
        await BroadcastFlightStatuses();
        await base.OnConnectedAsync();
    }
}