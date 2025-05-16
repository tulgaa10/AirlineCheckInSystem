using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

public partial class MainForm : Form
{
    private readonly HttpClient _http = new();
    private HubConnection _hub;
    private Passenger? _currentPassenger;

    public MainForm()
    {
        InitializeComponent();
        _http.BaseAddress = new Uri("http://localhost:5000/api/"); // Adjust if needed
        SetupSignalR();
    }

    private async void SetupSignalR()
    {
        _hub = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/flighthub")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<List<FlightUpdateDto>>("ReceiveFlightUpdates", updates =>
        {
            var status = updates.FirstOrDefault(u => u.FlightNumber == _currentPassenger?.Flight?.FlightNumber)?.Status;
            if (status != null)
            {
                Invoke(() => lblStatus.Text = $"Flight Status: {status}");
            }
        });

        await _hub.StartAsync();
    }
    private async void btnSearch_Click(object sender, EventArgs e)
    {
        string passport = txtPassport.Text;
        _currentPassenger = await _http.GetFromJsonAsync<Passenger>($"CheckIn/find-passenger/{passport}");

        if (_currentPassenger != null)
        {
            MessageBox.Show($"Passenger found: {_currentPassenger.FullName}");
            await LoadAvailableSeats(_currentPassenger.FlightId);
        }
        else
        {
            MessageBox.Show("Passenger not found.");
        }
    }

    private async Task LoadAvailableSeats(int flightId)
    {
        var flight = await _http.GetFromJsonAsync<Flight>($"Flights/{flightId}");
        lstSeats.Items.Clear();
        foreach (var seat in flight.Seats.Where(s => !s.IsAssigned))
        {
            lstSeats.Items.Add(seat.SeatNumber);
        }
    }
    private async void btnAssign_Click(object sender, EventArgs e)
    {
        string seatNumber = lstSeats.SelectedItem?.ToString();
        if (seatNumber == null || _currentPassenger == null) return;

        var response = await _http.PostAsync(
            $"Passengers/{_currentPassenger.Id}/assign-seat",
            JsonContent.Create(seatNumber));

        if (response.IsSuccessStatusCode)
        {
            MessageBox.Show("Seat assigned!");
        }
        else
        {
            MessageBox.Show("Failed to assign seat.");
        }
    }
    private void btnPrint_Click(object sender, EventArgs e)
    {
        if (_currentPassenger == null) return;

        string pass = $"BOARDING PASS\n" +
                      $"Name: {_currentPassenger.FullName}\n" +
                      $"Passport: {_currentPassenger.PassportNumber}\n" +
                      $"Flight: {_currentPassenger.Flight?.FlightNumber}\n" +
                      $"Seat: {_currentPassenger.Seat?.SeatNumber}\n" +
                      $"Status: {lblStatus.Text}";

        File.WriteAllText("boarding_pass.txt", pass);
        MessageBox.Show("Boarding pass printed (saved to file).");
    }
    private async void btnUpdate_Click(object sender, EventArgs e)
    {
        if (_currentPassenger?.Flight == null) return;

        var status = (FlightStatus)Enum.Parse(typeof(FlightStatus), cmbStatus.SelectedItem.ToString());
        var dto = new { FlightId = _currentPassenger.Flight.Id, NewStatus = status };

        var response = await _http.PostAsJsonAsync("Flights/update-status", dto);
        if (response.IsSuccessStatusCode)
        {
            MessageBox.Show("Flight status updated.");
            await _hub.InvokeAsync("BroadcastFlightStatuses"); // Notify all clients
        }
    }

}
