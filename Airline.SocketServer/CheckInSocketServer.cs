using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Airline.DataAccess;

public class CheckInSocketServer
{   
    private readonly AirlineDbContext _dbContext;
    private readonly TcpListener _listener;

    public CheckInSocketServer(AirlineDbContext dbContext, int port = 8888)
    {
        _dbContext = dbContext;
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        Console.WriteLine("Check-In Socket Server started...");

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer);
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        var parts = request.Split("|");
        if (parts.Length != 2)
        {
            await SendResponse(stream, "Invalid request format");
            return;
        }

        string seatNumber = parts[0];
        int passengerId = int.Parse(parts[1]);

        // Concurrency-safe check-in
        try
        {
            var seat = await _dbContext.Seats
                .Include(s => s.Passenger)
                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);

            if (seat == null)
            {
                await SendResponse(stream, "Seat not found");
                return;
            }

            if (seat.IsAssigned)
            {
                await SendResponse(stream, "Seat already taken");
                return;
            }

            seat.IsAssigned = true;
            seat.PassengerId = passengerId;
            await _dbContext.SaveChangesAsync();

            await SendResponse(stream, "Seat assigned successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            await SendResponse(stream, "Concurrency conflict occurred");
        }
        catch (Exception ex)
        {
            await SendResponse(stream, "Error: " + ex.Message);
        }
    }

    private async Task SendResponse(NetworkStream stream, string message)
    {
        var response = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(response, 0, response.Length);
    }
}
