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

        [BindProperty] public string TrainId { get; set; }
        [BindProperty] public string SelectedClass { get; set; }
        [BindProperty] public string TravelDateString { get; set; }
        [BindProperty] public decimal TotalPayable { get; set; }
        [BindProperty] public string SeatsRawData { get; set; }

        [BindProperty] public string CustomerName { get; set; }
        [BindProperty] public string CustomerPhone { get; set; }
        [BindProperty] public string CustomerCNIC { get; set; }

        public List<string> SelectedSeatsList { get; set; } = new List<string>();

        public CheckoutModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        // Selected seats se initial validation params pull down krna via checkbox states POST redirect
        public void OnGet()
        {
            // Failback wrapper handling fallback errors
            ExtractSeatsList();
        }

        public IActionResult OnPost()
        {
            ExtractSeatsList();

            if (string.IsNullOrEmpty(CustomerName) || string.IsNullOrEmpty(CustomerPhone))
            {
                return Page();
            }

            var train = _trainCollection.Find(t => t.Id == TrainId).FirstOrDefault();
            if (train == null) return RedirectToPage("/Customer/Index");

            DateTime.TryParse(TravelDateString, out DateTime parsedDate);

            // ?? Dynamic Unique Ticket PNR Generator Engine
            string trackingPNR = "SAFAR-" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper() + "-" + new Random().Next(100, 999);

            // ??? Formulate Domain Booking Object
            var finalBooking = new Booking
            {
                TicketNumber = trackingPNR,
                TrainId = train.Id,
                TrainName = train.Name,
                SourceStation = train.DefaultSource,
                DestinationStation = train.DefaultDestination,
                TravelDate = parsedDate,
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                CustomerCNIC = CustomerCNIC,
                SelectedClass = SelectedClass,
                BookedSeats = SelectedSeatsList,
                TotalFare = TotalPayable,
                BookingTimestamp = DateTime.Now,
                PaymentStatus = "Paid"
            };

            // Commit transaction layer directly inside database cluster mapping
            _bookingCollection.InsertOne(finalBooking);

            // Pass tracking variables into final printable receipt frame
            return RedirectToPage("./TicketConfirmation", new { ticketId = finalBooking.Id });
        }

        private void ExtractSeatsList()
        {
            // Captures parameters safely passed from checkouts checkbox parameters
            var seatsQuery = Request.Method == "POST" ? SeatsRawData : Request.Query["selectedSeats"].ToString();
            SeatsRawData = seatsQuery;

            if (!string.IsNullOrEmpty(seatsQuery))
            {
                SelectedSeatsList = seatsQuery.Split(',').Select(s => s.Trim()).ToList();
            }

            if (Request.Method == "GET")
            {
                TrainId = Request.Query["trainId"];
                SelectedClass = Request.Query["selectedClass"];
                TravelDateString = Request.Query["travelDate"];

                decimal.TryParse(Request.Query["baseFare"], out decimal baseFare);
                TotalPayable = SelectedSeatsList.Count * baseFare;
            }
        }
    }
}