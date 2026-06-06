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

            // 1. AAPKI PRICING LOGIC CONFIGURATION
            // Agar aapke scheduling dashboard par static integer base price assume ki ho to use filter karein, 
            // yahan simulation standard base pricing setup apply kiya gaya hai:
            decimal defaultSchedulingFare = 1500;

            if (@class == "Economy")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Economy?.SeatsPerBogie ?? 72;
                BaseFare = defaultSchedulingFare;
            }
            else if (@class == "Business")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Business?.SeatsPerBogie ?? 48;
                BaseFare = defaultSchedulingFare * 1.20m; // 20% Add-on Multiplier Matrix
            }
            else if (@class == "Executive")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Executive?.SeatsPerBogie ?? 30;
                BaseFare = defaultSchedulingFare * 1.50m; // 50% Add-on Multiplier Matrix
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