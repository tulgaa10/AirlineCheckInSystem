using Microsoft.AspNetCore.SignalR.Client;
using System.Drawing.Printing;
using System.Net.Http.Json;
namespace Airline.WinApp
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _http = new();
        private HubConnection _hub;
        private Passenger? _currentPassenger;
        private PrintDocument printDocument = new PrintDocument();
        private string boardingPassText = string.Empty;

        public Form1()
        {
            printDocument.PrintPage += PrintDocument_PrintPage;
            InitializeComponent();
            _http.BaseAddress = new Uri("https://localhost:7257/api/"); // Adjust if needed
            SetupSignalR();
        }

        private async void SetupSignalR()
        {
            _hub = new HubConnectionBuilder()
                .WithUrl("https://localhost:7257/flighthub")
                .WithAutomaticReconnect()
                .Build();

            _hub.On<List<FlightStatusUpdateDto>>("ReceiveFlightUpdates", updates =>
            {
                var status = updates.FirstOrDefault(u => u.FlightId == _currentPassenger?.FlightId)?.Status;
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

            var response = await _http.PostAsJsonAsync(
                $"Passengers/{_currentPassenger.Id}/assign-seat",
                seatNumber);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Seat assigned!");
                await LoadAvailableSeats(_currentPassenger.FlightId);  // Refresh the seat list
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Failed to assign seat: {errorMessage}");
            }
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_currentPassenger == null) return;

            boardingPassText = $"BOARDING PASS\n" +
                              $"Name: {_currentPassenger.FullName}\n" +
                              $"Passport: {_currentPassenger.PassportNumber}\n" +
                              $"Flight: {_currentPassenger.Flight?.FlightNumber}\n" +
                              $"Seat: {_currentPassenger.Seat?.SeatNumber}\n" +
                              $"Status: {lblStatus.Text}";

            try
            {
                printDocument.Print();
                MessageBox.Show("Boarding pass printed.");
            }
            catch (Exception ex)
            {
                // Fallback to file save if printing fails
                File.WriteAllText("boarding_pass.txt", boardingPassText);
                MessageBox.Show("Boarding pass saved to file.");
            }
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
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(boardingPassText, new Font("Arial", 12), Brushes.Black, new PointF(100, 100));
        }



    }

}
