using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class FlightHub : Hub
{
    private readonly AirlineDbContext _db;

    public FlightHub(AirlineDbContext db) => _db = db;

    public async Task BroadcastFlightStatuses()
    {
        var updates = await _db.Flights
            .Select(f => new FlightStatusUpdateDto
            {
                FlightId = f.Id,
                FlightNumber = f.FlightNumber,
                Status = f.Status.ToString()
            })
            .ToListAsync();
        await Clients.All.SendAsync("ReceiveFlightUpdates", updates);
    }
}
