using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;

namespace Safar.Pages.Customer
{
    public class SelectSeatsModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;
        private readonly IMongoCollection<Booking> _bookingCollection;

        public Train CurrentTrain { get; set; } = new Train();
        public string SelectedClass { get; set; }
        public string TravelDateString { get; set; }

        public int TotalSeatsCount { get; set; }
        public decimal BaseFare { get; set; }

        // ?? URL PARAMETERS BINDING GUARDS (Bina purane code ko chede dynamic tracking lagayi hai)
        [BindProperty(SupportsGet = true)]
        public string Src { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Dest { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal Fare { get; set; } // Tracks exact calculation sent from search grid

        public List<string> ExistingBookedSeats { get; set; } = new List<string>();

        public SelectSeatsModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public IActionResult OnGet(string id, string @class, string date)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(@class))
            {
                return RedirectToPage("/Customer/Index");
            }

            CurrentTrain = _trainCollection.Find(t => t.Id == id).FirstOrDefault();
            if (CurrentTrain == null)
            {
                return RedirectToPage("/Customer/Index");
            }

            SelectedClass = @class;
            TravelDateString = date;

            // 1. AAPKI PRICING LOGIC CONFIGURATION (Dynamic Parameter Fallback Added)
            // Agar piche se exact parameter scale fare mil raha hai to wahi chalega, nahi to default mapping:
            decimal defaultSchedulingFare = Fare > 0 ? Fare : 1500;

            if (@class == "Economy")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Economy?.SeatsPerBogie ?? 72;
                BaseFare = defaultSchedulingFare;
            }
            else if (@class == "Business")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Business?.SeatsPerBogie ?? 48;

                // Agar piche se exact fare calculated aa rahi hai to multiply dobara na karein, direct apply ho
                BaseFare = Fare > 0 ? Fare : (defaultSchedulingFare * 1.20m);
            }
            else if (@class == "Executive")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Executive?.SeatsPerBogie ?? 30;

                // Same logic for executive class pricing
                BaseFare = Fare > 0 ? Fare : (defaultSchedulingFare * 1.50m);
            }

            // 2. REAL-TIME DOUBLE BOOKING CONCURRENCY GUARD FILTER
            // Check mapping inside Bookings Collection to pull down already preserved berths
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                var activeBookings = _bookingCollection.Find(b =>
                    b.TrainId == CurrentTrain.Id &&
                    b.TravelDate.Date == parsedDate.Date &&
                    b.SelectedClass == @class
                ).ToList();

                foreach (var booking in activeBookings)
                {
                    if (booking.BookedSeats != null)
                    {
                        ExistingBookedSeats.AddRange(booking.BookedSeats);
                    }
                }
            }

            return Page();
        }
    }
}