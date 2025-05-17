using Microsoft.EntityFrameworkCore;

public class FlightRepository : IFlightRepository
{
    private readonly AirlineDbContext _context;

    public FlightRepository(AirlineDbContext context)
    {
        _context = context;
    }

    public async Task<List<Flight>> GetAllAsync()
    {
        return await _context.Flights.Include(f => f.Seats).Include(f => f.Passengers).ToListAsync();
    }

    public async Task<Flight?> GetByIdAsync(int id)
    {
        return await _context.Flights.Include(f => f.Seats).Include(f => f.Passengers).FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task UpdateStatusAsync(int id, FlightStatus status)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight != null)
        {
            flight.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}