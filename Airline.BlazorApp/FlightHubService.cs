using Microsoft.AspNetCore.SignalR.Client;

public class FlightHubService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    public event Action<List<FlightStatusUpdateDto>>? OnFlightStatusUpdate;

    public FlightHubService(string hubUrl)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<List<FlightStatusUpdateDto>>("ReceiveFlightUpdates", updates =>
        {
            OnFlightStatusUpdate?.Invoke(updates);
        });
    }

    public async Task StartAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
            await _hubConnection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hubConnection.State != HubConnectionState.Disconnected)
            await _hubConnection.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _hubConnection.DisposeAsync();
    }
}