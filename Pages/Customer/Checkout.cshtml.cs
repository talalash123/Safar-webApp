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

        [BindProperty] public string CustomerName { get; set; }
        [BindProperty] public string CustomerPhone { get; set; }
        [BindProperty] public string CustomerCNIC { get; set; }

        public List<string> SelectedSeatsList { get; set; } = new List<string>();

        public CheckoutModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public void OnGet()
        {
            ExtractSeatsList();
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

            if (string.IsNullOrEmpty(CustomerName) || string.IsNullOrEmpty(CustomerPhone))
            {
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
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                CustomerCNIC = CustomerCNIC,
                SelectedClass = SelectedClass,
                BookedSeats = SelectedSeatsList,

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