using Microsoft.AspNetCore.SignalR.Client;
using System.Drawing.Printing;
using System.Net.Http.Json;
using System.Text.Json;

namespace Airline.WinApp
{
    public partial class Form1 : Form
    {
        // API haihad ashiglah HTTP client
        private readonly HttpClient _http = new();

        // Real-time medeelel damjuulah SignalR holbolt
        private HubConnection? _hub;

        // Odoogiin songogdson zorchigchiin medeelel
        private Passenger? _currentPassenger;

        // Boarding pass hevleh document
        private readonly PrintDocument printDocument = new PrintDocument();

        // Boarding pass-d haruulah text
        private string boardingPassText = string.Empty;

        public Form1()
        {
            // Hevleh uildeld shaardalgatai event handler nemeh
            printDocument.PrintPage += PrintDocument_PrintPage;

            // Form-iin component-uudiig ehluuleh
            InitializeComponent();

            // API-iin hayag todorhoiloh
            _http.BaseAddress = new Uri("http://192.168.137.1:7257/api/");

            // Aldaa garsniig barihaad haruulah event
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Unhandled error: {e.ExceptionObject}");
            };

            // Form-iin Control-uudiig ehluuleh
            InitializeControls();

            // SignalR holbolt uusgeh
            SetupSignalR();

            // Niisleguudiin medeelliig tataj avah
            LoadFlightsAsync();
        }

        // Form-iin control-uudiig ehluuleh
        private void InitializeControls()
        {
            // Nislegiin tuluv songoh dropdown-d utguudiig nemeh
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Registering");
            cmbStatus.Items.Add("Boarding");
            cmbStatus.Items.Add("Departed");
            cmbStatus.Items.Add("Delayed");
            cmbStatus.Items.Add("Cancelled");
            cmbStatus.SelectedIndex = 0;

            // Ehlel baidal zaaj ogoh
            lblStatus.Text = "Flight Status: Not selected";
        }

        // SignalR holbolt uusgeh
        private async void SetupSignalR()
        {
            try
            {
                // SignalR holbolt uusgeh
                _hub = new HubConnectionBuilder()
                    .WithUrl("http://192.168.137.1:7257/flighthub")
                    .WithAutomaticReconnect()
                    .Build();

                // Niislegiin status oorchlogdson ued duudagdah handler
                _hub.On<List<FlightStatusUpdateDto>>("ReceiveFlightUpdates", updates =>
                {
                    // Odoogiin songogdson zorchigchiin niislegiin status oorchlogdson bga esehiig shalgah
                    var status = updates.FirstOrDefault(u => u.FlightId == _currentPassenger?.FlightId)?.Status;
                    if (status != null)
                    {
                        // UI-g shinechlehiin tuld Form thread deer duudah
                        Invoke(() => lblStatus.Text = $"Flight Status: {status}");
                    }
                });

                // Holbolt taslarsniig barihaad dahin holbogdoh handler
                _hub.Closed += async (error) =>
                {
                    Invoke(() => lblStatus.Text = "SignalR: Disconnected");
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await _hub.StartAsync();
                };

                // SignalR server luu holbogdoh
                await _hub.StartAsync();

                // Holboltiin baidal haruulah
                lblStatus.Text = "SignalR: Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SignalR Connection Error: {ex.Message}");
            }
        }

        // Niisleguudiin jagsaaltiig tataj avah
        private async void LoadFlightsAsync()
        {
            try
            {
                // API-aas buh niisleguudiig tataj avah
                var response = await _http.GetAsync("Flights");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // JSON deserialize hiih tohirgoo
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    // JSON-g Flight object-uudiin list bolgoj hurvuuleh
                    var flights = JsonSerializer.Deserialize<List<Flight>>(json, options);

                    // Dropdown-iig tseverleed shineer niisleguudiig nemeh
                    cmbFlights.Items.Clear();

                    foreach (var flight in flights!)
                    {
                        cmbFlights.Items.Add(flight);
                    }

                    // Dropdown-d haruulah bolon value-giin field-uudiig todorhoiloh
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

        // Zorchigch haih tovchii event handler
        private async void btnSearch_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Passport dugaariig TextBox-oos avah
                string passport = txtPassport.Text;
                if (string.IsNullOrWhiteSpace(passport))
                {
                    MessageBox.Show("Please enter a passport number");
                    return;
                }

                // API-aas zorchigchiig haih huselt ilgeeh
                var response = await _http.GetAsync($"CheckIn/find-passenger/{passport}");
                if (response.IsSuccessStatusCode)
                {
                    // JSON response-g Passenger object bolgoj hurvuuleh
                    _currentPassenger = await response.Content.ReadFromJsonAsync<Passenger>();
                    if (_currentPassenger != null)
                    {
                        MessageBox.Show($"Passenger found: {_currentPassenger.FullName}");

                        // Songogdson niislegiin boolomjit suudluudiig haruulah
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

        // Niislegiin boolomjit suudluudiig haruulah
        private async Task LoadAvailableSeats(int flightId)
        {
            try
            {
                // Jagsaaltiig tseverleh
                lstSeats.Items.Clear();

                // Tataj avch baigaa medegdel
                lstSeats.Items.Add("Loading seats...");

                // API-aas niislegiin medeelliig tataj avah
                var response = await _http.GetAsync($"Flights/{flightId}");

                // Loading message-iig arilgah
                lstSeats.Items.Clear();

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    // Debug: Hariu bolson JSON-g file bolgon hadgalah
                    File.WriteAllText("flight_response.json", json);
                    Console.WriteLine($"API Response: {json}");

                    // JSON deserialize hiih tohirgoo
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    // JSON-g Flight object bolgoj hurvuuleh
                    var flight = JsonSerializer.Deserialize<Flight>(json, options);

                    // Debug medeelel haruulah
                    MessageBox.Show($"Deserialized flight {flight?.Id}. Seats count: {flight?.Seats?.Count ?? 0}");

                    // Suudluud baival tedgeeriig shalgaj boolomjitoig haruulah
                    if (flight?.Seats != null && flight.Seats.Any())
                    {
                        int availableCount = 0;
                        foreach (var seat in flight.Seats)
                        {
                            // Debug: Suudal bolgonii medeelliig haruulah
                            Console.WriteLine($"Seat: {seat.SeatNumber}, IsAssigned: {seat.IsAssigned}");

                            // Onoogdoogui suudluudiig jagsaaltad nemeh
                            if (!seat.IsAssigned)
                            {
                                lstSeats.Items.Add(seat.SeatNumber);
                                availableCount++;
                            }
                        }

                        // Herev boolomjit suudal baihgui bol medegdel haruulah
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

        // Suudal onooh tovchiin event handler
        private async void btnAssign_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Songogdson suudliin dugaariig avah
                string? seatNumber = lstSeats.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(seatNumber) || _currentPassenger == null)
                {
                    MessageBox.Show("Please select a seat and search for a passenger first");
                    return;
                }

                // API ruu suudal onooh huselt ilgeeh
                var response = await _http.PostAsJsonAsync(
                    $"CheckIn/{_currentPassenger.Id}/assign-seat",
                    seatNumber);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Seat assigned!");

                    // Jagsaaltiig dahin shinechleh
                    await LoadAvailableSeats(_currentPassenger.FlightId);
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

        // Boarding pass hevleh tovchiin event handler
        private async void btnPrint_Click_1(object sender, EventArgs e)
        {
            if (_currentPassenger == null)
            {
                MessageBox.Show("Please search for a passenger first");
                return;
            }

            try
            {
                // Zorchigchiin nislegiin dugaariig avah
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

                // Zorchigchiin medeelliig shinechleh (suudliin medeeleliin hamt)
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

                // Zorchigchiin suudliin medeelliig avah
                string seatNumber = _currentPassenger?.Seat?.SeatNumber ?? "Not Assigned";

                // Debug medeelel
                Console.WriteLine($"Flight ID: {_currentPassenger?.FlightId}, Flight Number: {flightNumber}");
                Console.WriteLine($"Seat Number: {seatNumber}");

                // Boarding pass-iin text-iig uusgeh
                boardingPassText = $"BOARDING PASS\n" +
                                  $"Name: {_currentPassenger.FullName}\n" +
                                  $"Passport: {_currentPassenger.PassportNumber}\n" +
                                  $"Flight: {flightNumber}\n" +
                                  $"Seat: {seatNumber}\n" +
                                  $"Status: {lblStatus.Text}";

                try
                {
                    // Boarding pass-iig default printer ruu ilgeeh
                    printDocument.Print();
                    MessageBox.Show("Boarding pass printed.");
                }
                catch (Exception ex)
                {
                    // Herev hevleh bolomjgui bol file ruu hadgalah
                    File.WriteAllText("boarding_pass.txt", boardingPassText);
                    MessageBox.Show($"Printing failed: {ex.Message}\nBoarding pass saved to file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error preparing boarding pass: {ex.Message}");
            }
        }

        // Nislegiin tuluv oorchiloh tovchiin event handler
        private async void btnUpdate_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Songogdson nisleg bolon tuluv baigaa esehiig shalgah
                if (cmbFlights.SelectedItem is not Flight selectedFlight || cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Please search for a passenger and select a status");
                    return;
                }

                // Tuluviin enum utgiig songogdson utgiig ashiglan avah
                var status = (FlightStatus)Enum.Parse(typeof(FlightStatus), cmbStatus.SelectedItem.ToString()!);

                // Nislegiin tuluv oorchiloh DTO-g uusgeh
                var dto = new FlightStatusChangeRequestDto
                {
                    FlightId = selectedFlight.Id,
                    NewStatus = status
                };

                // API ruu nislegiin status oorchiloh huselt ilgeeh
                var response = await _http.PostAsJsonAsync("Flights/update-status", dto);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Flight status updated.");

                    // SignalR ruu broadcast hiih huselt ilgeeh
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

        // Boarding pass hevleh uyd ajillah handler
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Graphics deer text-iig zurah
            e.Graphics.DrawString(boardingPassText, new Font("Arial", 12), Brushes.Black, new PointF(100, 100));
        }

        // Debug tovchiin event handler
        private async void btnDebug_Click(object sender, EventArgs e)
        {
            // Zorchigch songogdson eseh shalgah
            if (_currentPassenger?.FlightId == null)
            {
                MessageBox.Show("Please search for a passenger first");
                return;
            }

            try
            {
                // Debug API ruu huselt ilgeeh
                var response = await _http.GetAsync($"Flights/debug/{_currentPassenger.FlightId}");
                if (response.IsSuccessStatusCode)
                {
                    // Hariug file ruu hadgalah
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

    // API huseltend ashiglah model bolon DTO angilluud

    // Zorchigchiin model
    public class Passenger
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }
        public Seat? Seat { get; set; }
    }

    // Nislegiin model
    public class Flight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public FlightStatus Status { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }

    // Suudliin model
    public class Seat
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    // Nislegiin tuluv enum
    public enum FlightStatus
    {
        Registering,
        Boarding,
        Departed,
        Delayed,
        Cancelled
    }

    // Nislegiin tuluv oorchlogdson tuhain DTO
    public class FlightStatusUpdateDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // Nislegiin tuluv oorchiloh huseltiin DTO
    public class FlightStatusChangeRequestDto
    {
        public int FlightId { get; set; }
        public FlightStatus NewStatus { get; set; }
    }
}