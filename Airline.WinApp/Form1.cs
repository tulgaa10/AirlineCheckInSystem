using Microsoft.AspNetCore.SignalR.Client;
using System.Drawing.Printing;
using System.Net.Http.Json;
using System.Text.Json;

namespace Airline.WinApp
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _http = new();
        private HubConnection? _hub;
        private Passenger? _currentPassenger;
        private readonly PrintDocument printDocument = new PrintDocument();
        private string boardingPassText = string.Empty;

        public Form1()
        {
            printDocument.PrintPage += PrintDocument_PrintPage;
            InitializeComponent();

            // Explicitly set the API URL
            _http.BaseAddress = new Uri("https://localhost:7257/api/");

            // Add error handler
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Unhandled error: {e.ExceptionObject}");
            };

            InitializeControls();
            SetupSignalR();
            LoadFlightsAsync();
        }

        private void InitializeControls()
        {
            // Add flight status options to the dropdown
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Registering");
            cmbStatus.Items.Add("Boarding");
            cmbStatus.Items.Add("Departed");
            cmbStatus.Items.Add("Delayed");
            cmbStatus.Items.Add("Cancelled");
            cmbStatus.SelectedIndex = 0;

            lblStatus.Text = "Flight Status: Not selected";
        }

        private async void SetupSignalR()
        {
            try
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

                // Add explicit handlers for connection events
                _hub.Closed += async (error) =>
                {
                    Invoke(() => lblStatus.Text = "SignalR: Disconnected");
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await _hub.StartAsync();
                };

                await _hub.StartAsync();
                // Display connection status
                lblStatus.Text = "SignalR: Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SignalR Connection Error: {ex.Message}");
            }
        }
        private async void LoadFlightsAsync()
        {
            try
            {
                var response = await _http.GetAsync("Flights");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var flights = JsonSerializer.Deserialize<List<Flight>>(json, options);

                    cmbFlights.Items.Clear();

                    foreach (var flight in flights!)
                    {
                        cmbFlights.Items.Add(flight);
                    }

                    cmbFlights.DisplayMember = "FlightNumber";
                    cmbFlights.ValueMember = "Id";
                }
                else
                {
                    MessageBox.Show("Failed to load flights.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Flight load error: {ex.Message}");
            }
        }

        private async void btnSearch_Click_1(object sender, EventArgs e)
        {
            try
            {
                string passport = txtPassport.Text;
                if (string.IsNullOrWhiteSpace(passport))
                {
                    MessageBox.Show("Please enter a passport number");
                    return;
                }

                var response = await _http.GetAsync($"CheckIn/find-passenger/{passport}");
                if (response.IsSuccessStatusCode)
                {
                    _currentPassenger = await response.Content.ReadFromJsonAsync<Passenger>();
                    if (_currentPassenger != null)
                    {
                        MessageBox.Show($"Passenger found: {_currentPassenger.FullName}");
                        await LoadAvailableSeats(_currentPassenger.FlightId);
                    }
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search Error: {ex.Message}");
            }
        }

        private async Task LoadAvailableSeats(int flightId)
        {
            try
            {
                // Clear the list before adding new items
                lstSeats.Items.Clear();

                // Show loading message
                lstSeats.Items.Add("Loading seats...");

                var response = await _http.GetAsync($"Flights/{flightId}");

                // Remove loading message
                lstSeats.Items.Clear();

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Debug: Write the raw JSON to a file for inspection
                    File.WriteAllText("flight_response.json", json);
                    Console.WriteLine($"API Response: {json}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var flight = JsonSerializer.Deserialize<Flight>(json, options);

                    // Debug information
                    MessageBox.Show($"Deserialized flight {flight?.Id}. Seats count: {flight?.Seats?.Count ?? 0}");

                    if (flight?.Seats != null && flight.Seats.Any())
                    {
                        int availableCount = 0;
                        foreach (var seat in flight.Seats)
                        {
                            // Debug each seat
                            Console.WriteLine($"Seat: {seat.SeatNumber}, IsAssigned: {seat.IsAssigned}");

                            if (!seat.IsAssigned)
                            {
                                lstSeats.Items.Add(seat.SeatNumber);
                                availableCount++;
                            }
                        }

                        if (availableCount == 0)
                        {
                            lstSeats.Items.Add("-- All seats are assigned --");
                        }
                    }
                    else
                    {
                        lstSeats.Items.Add("-- No seats available --");
                    }
                }
                else
                {
                    lstSeats.Items.Add("-- Error loading seats --");
                    MessageBox.Show($"Error loading flight: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                lstSeats.Items.Add("-- Error --");
                MessageBox.Show($"Seat Loading Error: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private async void btnAssign_Click_1(object sender, EventArgs e)
        {
            try
            {
                string? seatNumber = lstSeats.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(seatNumber) || _currentPassenger == null)
                {
                    MessageBox.Show("Please select a seat and search for a passenger first");
                    return;
                }

                var response = await _http.PostAsJsonAsync(
                    $"CheckIn/{_currentPassenger.Id}/assign-seat",
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
            catch (Exception ex)
            {
                MessageBox.Show($"Seat Assignment Error: {ex.Message}");
            }
        }

        private async void btnPrint_Click_1(object sender, EventArgs e)
        {
            if (_currentPassenger == null)
            {
                MessageBox.Show("Please search for a passenger first");
                return;
            }

            try
            {
                // Get flight details directly using the passenger's FlightId
                string flightNumber = "Unknown";
                if (_currentPassenger.FlightId > 0)
                {
                    var flightResponse = await _http.GetAsync($"Flights/{_currentPassenger.FlightId}");
                    if (flightResponse.IsSuccessStatusCode)
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        var flightJson = await flightResponse.Content.ReadAsStringAsync();
                        var flight = JsonSerializer.Deserialize<Flight>(flightJson, options);

                        if (flight != null)
                        {
                            flightNumber = flight.FlightNumber;
                        }
                    }
                }

                // Reload the passenger with all related data for the seat information
                var passengerResponse = await _http.GetAsync($"CheckIn/find-passenger/{_currentPassenger.PassportNumber}");
                if (passengerResponse.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var passengerJson = await passengerResponse.Content.ReadAsStringAsync();
                    _currentPassenger = JsonSerializer.Deserialize<Passenger>(passengerJson, options);
                }

                string seatNumber = _currentPassenger?.Seat?.SeatNumber ?? "Not Assigned";

                // Debug information
                Console.WriteLine($"Flight ID: {_currentPassenger?.FlightId}, Flight Number: {flightNumber}");
                Console.WriteLine($"Seat Number: {seatNumber}");

                boardingPassText = $"BOARDING PASS\n" +
                                  $"Name: {_currentPassenger.FullName}\n" +
                                  $"Passport: {_currentPassenger.PassportNumber}\n" +
                                  $"Flight: {flightNumber}\n" +
                                  $"Seat: {seatNumber}\n" +
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
                    MessageBox.Show($"Printing failed: {ex.Message}\nBoarding pass saved to file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing boarding pass: {ex.Message}");
            }
        }

        private async void btnUpdate_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (cmbFlights.SelectedItem is not Flight selectedFlight || cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Please search for a passenger and select a status");
                    return;
                }

                var status = (FlightStatus)Enum.Parse(typeof(FlightStatus), cmbStatus.SelectedItem.ToString()!);
                var dto = new FlightStatusChangeRequestDto
                {
                    FlightId = selectedFlight.Id,
                    NewStatus = status
                };

                var response = await _http.PostAsJsonAsync("Flights/update-status", dto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Flight status updated.");
                    if (_hub?.State == HubConnectionState.Connected)
                    {
                        await _hub.InvokeAsync("BroadcastFlightStatuses");
                    }
                }
                else
                {
                    MessageBox.Show($"Status update failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Status Update Error: {ex.Message}");
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(boardingPassText, new Font("Arial", 12), Brushes.Black, new PointF(100, 100));
        }

        private async void btnDebug_Click(object sender, EventArgs e)
        {
            if (_currentPassenger?.FlightId == null)
            {
                MessageBox.Show("Please search for a passenger first");
                return;
            }

            try
            {
                var response = await _http.GetAsync($"Flights/debug/{_currentPassenger.FlightId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    File.WriteAllText("debug_response.json", json);
                    MessageBox.Show($"Debug info saved to debug_response.json\n\nPreview:\n{json.Substring(0, Math.Min(json.Length, 500))}...");
                }
                else
                {
                    MessageBox.Show($"Debug API call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug Error: {ex.Message}");
            }
        }
    }

    // DTOs to ensure type safety with API requests
    public class Passenger
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }
        public Seat? Seat { get; set; }
    }

    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public FlightStatus Status { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }

    public class Seat
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    public enum FlightStatus
    {
        Registering,
        Boarding,
        Departed,
        Delayed,
        Cancelled
    }

    public class FlightStatusUpdateDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class FlightStatusChangeRequestDto
    {
        public int FlightId { get; set; }
        public FlightStatus NewStatus { get; set; }
    }

}