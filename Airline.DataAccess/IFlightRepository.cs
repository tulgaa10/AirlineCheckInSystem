public interface IFlightRepository
{
    Task<List<Flight>> GetAllAsync();
    Task<Flight?> GetByIdAsync(int id);
    Task UpdateStatusAsync(int id, FlightStatus status);
}