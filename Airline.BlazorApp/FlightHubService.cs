using Microsoft.AspNetCore.SignalR.Client;

// SignalR hub-tai holbogdoj, nislegiin statusiin oorchlooltiin talaarh 
// real-time medeelliig huleen avdag service
public class FlightHubService : IAsyncDisposable
{
    // SignalR hubiin holbolt
    private readonly HubConnection _hubConnection;

    // Nislegiin tuluviin oorchlooltiin talaar eventeer medeeleh
    public event Action<List<FlightStatusUpdateDto>>? OnFlightStatusUpdate;

    // Constructor - hubiin hayagiig parametreer avna
    public FlightHubService(string hubUrl)
    {
        // SignalR holbolt uusgeh
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()  // Holbolt tasrahad dahin holbogdoh
            .Build();

        // ReceiveFlightUpdates metodiig server-s duudagdah uyd ajillah handler nemeh
        _hubConnection.On<List<FlightStatusUpdateDto>>("ReceiveFlightUpdates", updates =>
        {
            // EventHandler baigaa esehiig shalgaad, updates-iig damjuulah
            OnFlightStatusUpdate?.Invoke(updates);
        });
    }

    // Hub ruu holbogdoh
    public async Task StartAsync()
    {
        // Zovhon holbolt tasarsan baidal deer shineer holbogdono
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
    }

    // Hub-aas salah
    public async Task StopAsync()
    {
        // Zovhon holboltoon heviin baidald baigaa uyd salah
        if (_hubConnection.State != HubConnectionState.Disconnected)
            await _hubConnection.StopAsync();
    }

    // IAsyncDisposable interface-iin implementatsi - resource ceverleh
    public async ValueTask DisposeAsync()
    {
        // Holbolt baigaa bol salah
        await StopAsync();

        // SignalR holboltiig ustgah
        await _hubConnection.DisposeAsync();
    }
}