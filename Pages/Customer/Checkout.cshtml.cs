using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Safar.Pages.Customer
{
    public class CheckoutModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;
        private readonly IMongoCollection<Booking> _bookingCollection;

        [BindProperty(Name = "trainId", SupportsGet = true)]
        public string TrainId { get; set; }

        [BindProperty(Name = "selectedClass", SupportsGet = true)]
        public string SelectedClass { get; set; }

        [BindProperty(Name = "travelDate", SupportsGet = true)]
        public string TravelDateString { get; set; }

        [BindProperty]
        public decimal TotalPayable { get; set; }

        [BindProperty(Name = "baseFare", SupportsGet = true)]
        public decimal BaseFare { get; set; }

        [BindProperty]
        public string SeatsRawData { get; set; }

        [BindProperty] public string LeadName { get; set; }
        [BindProperty] public string LeadPhone { get; set; }
        [BindProperty] public string LeadCNIC { get; set; }

        // 🧠 AI Demographic Fields for Smart Seating
        [BindProperty] public int LeadAge { get; set; } = 30;
        [BindProperty] public string LeadGender { get; set; } = "Male";
        [BindProperty] public bool IsWithFamily { get; set; } = false;

        [BindProperty] public int PassengerCount { get; set; } = 1;
        [BindProperty] public string PassengerManifest { get; set; }

        public List<PassengerManifestItem> ParsedPassengers { get; set; } = new List<PassengerManifestItem>();

        public List<string> SelectedSeatsList { get; set; } = new List<string>();

        public CheckoutModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public void OnGet()
        {
            ExtractSeatsList();
            ParseManifest();
        }

        private void ParseManifest()
        {
            if (!string.IsNullOrEmpty(PassengerManifest))
            {
                try
                {
                    ParsedPassengers = System.Text.Json.JsonSerializer.Deserialize<List<PassengerManifestItem>>(PassengerManifest) 
                                       ?? new List<PassengerManifestItem>();
                }
                catch
                {
                    ParsedPassengers = new List<PassengerManifestItem>();
                }
            }

            // Fallback: If manifest is empty but PassengerCount is set, add default items
            if (ParsedPassengers.Count == 0 && PassengerCount > 0)
            {
                ParsedPassengers.Add(new PassengerManifestItem { Name = LeadName ?? "Lead Passenger", Age = LeadAge, Gender = LeadGender });
                for (int i = 1; i < PassengerCount; i++)
                {
                    ParsedPassengers.Add(new PassengerManifestItem { Name = $"Passenger {i + 1}", Age = 30, Gender = "Male" });
                }
            }
        }

        public IActionResult OnPost(List<string> selectedSeats)
        {
            // Sync seats input list stream securely
            if (selectedSeats != null && selectedSeats.Count > 0)
            {
                SelectedSeatsList = selectedSeats;
                SeatsRawData = string.Join(",", selectedSeats);
            }
            else
            {
                ExtractSeatsList();
            }

            // Recovery pricing parsing safeguard from direct Form values
            if (BaseFare == 0 && !string.IsNullOrEmpty(Request.Form["baseFare"]))
            {
                decimal.TryParse(Request.Form["baseFare"], out decimal formFare);
                BaseFare = formFare;
            }

            // Absolute mathematical calculation block override
            if (SelectedSeatsList != null && SelectedSeatsList.Count > 0)
            {
                TotalPayable = SelectedSeatsList.Count * BaseFare;
            }

            if (string.IsNullOrEmpty(LeadName) || string.IsNullOrEmpty(LeadPhone))
            {
                ParseManifest();
                return Page();
            }

            var train = _trainCollection.Find(t => t.Id == TrainId).FirstOrDefault();
            if (train == null) return RedirectToPage("/Customer/Index");

            if (!DateTime.TryParse(TravelDateString, out DateTime parsedDate))
            {
                parsedDate = DateTime.Now;
            }

            // PNR Token formulation
            string trackingPNR = "SAFAR-" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper() + "-" + new Random().Next(100, 999);

            // ?? Setup booking container object bypassing any implicit conversion dropouts
            var finalBooking = new Booking
            {
                TicketNumber = trackingPNR,
                TrainId = train.Id,
                TrainName = train.Name ?? "Safar Express",
                SourceStation = train.DefaultSource ?? "Origin",
                DestinationStation = train.DefaultDestination ?? "Destination",
                TravelDate = parsedDate,
                CustomerName = LeadName,
                CustomerPhone = LeadPhone,
                CustomerCNIC = LeadCNIC,
                SelectedClass = SelectedClass,
                BookedSeats = SelectedSeatsList,

                // 🧠 AI Demographic Data for Smart Seating
                CustomerDetails = new CustomerDetailInfo
                {
                    Age = LeadAge > 0 ? LeadAge : 30,
                    Gender = !string.IsNullOrEmpty(LeadGender) ? LeadGender : "Male",
                    IsWithFamily = IsWithFamily
                },

                // SAFE VALUATION PIPELINE: Direct runtime multiplication guarantee
                TotalFare = TotalPayable > 0 ? TotalPayable : (SelectedSeatsList.Count * (BaseFare > 0 ? BaseFare : 1500)),

                BookingTimestamp = DateTime.Now,
                PaymentStatus = "Paid"
            };

            // Commit record inside database safely
            _bookingCollection.InsertOne(finalBooking);

            // Pass the absolute generated String ID to TicketConfirmation
            return RedirectToPage("./TicketConfirmation", new { ticketId = finalBooking.Id });
        }

        private void ExtractSeatsList()
        {
            var seatsQuery = Request.Method == "POST" ? Request.Form["selectedSeats"].ToString() : Request.Query["selectedSeats"].ToString();

            if (string.IsNullOrEmpty(seatsQuery) && Request.Method == "POST")
            {
                seatsQuery = SeatsRawData;
            }

            SeatsRawData = seatsQuery;

            if (!string.IsNullOrEmpty(seatsQuery))
            {
                SelectedSeatsList = seatsQuery.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

            if (Request.Method == "GET")
            {
                TrainId = Request.Query["trainId"];
                SelectedClass = Request.Query["selectedClass"];
                TravelDateString = Request.Query["travelDate"];

                decimal.TryParse(Request.Query["baseFare"], out decimal parsedBaseFare);
                BaseFare = parsedBaseFare;
                TotalPayable = SelectedSeatsList.Count * BaseFare;
            }
            else
            {
                if (BaseFare == 0)
                {
                    decimal.TryParse(Request.Form["baseFare"], out decimal formBaseFare);
                    BaseFare = formBaseFare;
                }
                TotalPayable = SelectedSeatsList.Count * BaseFare;
            }
        }
    }
}