using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Airline.DataAccess;

// Socket server angilal - TCP protocol ashiglan client-uudtai holbogdoj,
// suudal onoogdoh huselt huleej avna.
public class CheckInSocketServer
{
    // DB-tei ajillah context
    private readonly AirlineDbContext _dbContext;

    // TCP client sonsoh listener
    private readonly TcpListener _listener;

    // Constructor - ogogdliin sangiin context bolon port dugaar avna
    public CheckInSocketServer(AirlineDbContext dbContext, int port = 8888)
    {
        _dbContext = dbContext;
        // Buh interface deer (IPAddress.Any) zaasan port-oor sonsoh
        _listener = new TcpListener(IPAddress.Any, port);
    }

    // Server ehluuleh method
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Listener ehluuleh
        _listener.Start();
        Console.WriteLine("Check-In Socket Server started...");

        // Tokennoor tsutslaltgui client-uud huleej avah
        while (!cancellationToken.IsCancellationRequested)
        {
            // Shine client holbogdohiig huleeh
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);

            // Client bolgoniig tusgai thread deer ajluulah (fire-and-forget)
            _ = HandleClientAsync(client);
        }
    }

    // Client bolgonii huselt bolovsruulah
    private async Task HandleClientAsync(TcpClient client)
    {
        // Stream-iig using block-d oruulj, automatic disposh hiih
        using var stream = client.GetStream();

        // Client-ees irsen data unshih buffer
        var buffer = new byte[1024];

        // Client-ees irsen datag unshih
        var bytesRead = await stream.ReadAsync(buffer);

        // Byte[] array-g string bolgoj hurvuuleh
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Request format: "SeatNumber|PassengerId"
        var parts = request.Split("|");

        // Format zov esehiig shalgah
        if (parts.Length != 2)
        {
            await SendResponse(stream, "Invalid request format");
            return;
        }

        // Request-iin utgig zadlah
        string seatNumber = parts[0];
        int passengerId = int.Parse(parts[1]);

        // Concurrent check-in uildel - olon hereglegch negin zoroo suudal songoh
        // uyd gardag asuudliig shiidverlehiin tuld
        try
        {
            // Ogson suudliin dugaaraar suudliig haih (zorchigchtoi ni tsug)
            var seat = await _dbContext.Seats
                .Include(s => s.Passenger)
                .FirstOrDefaultAsync(s => s.SeatNumber == seatNumber);

            // Suudal oldoogui bol
            if (seat == null)
            {
                await SendResponse(stream, "Seat not found");
                return;
            }

            // Herev suudal urd ni onoogdson bol
            if (seat.IsAssigned)
            {
                await SendResponse(stream, "Seat already taken");
                return;
            }

            // Suudliig zorchigchid onoogdson gej temdegleh
            seat.IsAssigned = true;
            seat.PassengerId = passengerId;

            // Ogogdliin sand hadgalah
            await _dbContext.SaveChangesAsync();

            // Amjilttai bolson tuhain client-d hariu ilgeeh
            await SendResponse(stream, "Seat assigned successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Concurrency exception - oor hereglegch tuun zoroo ene suudliig avsan
            await SendResponse(stream, "Concurrency conflict occurred");
        }
        catch (Exception ex)
        {
            // Busad aldaani medeeg butsaah
            await SendResponse(stream, "Error: " + ex.Message);
        }
    }

    // Client ruu hariu ilgeeh helper method
    private async Task SendResponse(NetworkStream stream, string message)
    {
        // String-iig byte[] bolgoj hurvuuleh
        var response = Encoding.UTF8.GetBytes(message);

        // Stream ruu hariu bichih
        await stream.WriteAsync(response, 0, response.Length);
    }
}