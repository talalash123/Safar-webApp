using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public int TotalBogiesCount { get; set; } = 1;
        public decimal BaseFare { get; set; }
        public string DisplayTrainCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Src { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Dest { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal Fare { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CurrentBogie { get; set; }

        public List<string> ExistingBookedSeats { get; set; } = new List<string>();

        public SelectSeatsModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public IActionResult OnGet(string id, string @class, string date, string bogie)
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

            // Safe reflection check backup configuration
            DisplayTrainCode = !string.IsNullOrEmpty(CurrentTrain.TrainId)
                ? CurrentTrain.TrainId
                : (CurrentTrain.Id != null && CurrentTrain.Id.Length > 5
                    ? CurrentTrain.Id.Substring(CurrentTrain.Id.Length - 5).ToUpper()
                    : "T-REG");

            decimal defaultSchedulingFare = Fare > 0 ? Fare : 1500;
            string prefix = @class.Substring(0, 1).ToUpper();

            if (@class == "Economy")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Economy?.SeatsPerBogie ?? 72;
                TotalBogiesCount = CurrentTrain.ClassDistribution?.Economy?.BogiesCount ?? 3;
                BaseFare = defaultSchedulingFare;
            }
            else if (@class == "Business")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Business?.SeatsPerBogie ?? 48;
                TotalBogiesCount = CurrentTrain.ClassDistribution?.Business?.BogiesCount ?? 2;
                BaseFare = Fare > 0 ? Fare : (defaultSchedulingFare * 1.40m);
            }
            else if (@class == "Executive")
            {
                TotalSeatsCount = CurrentTrain.ClassDistribution?.Executive?.SeatsPerBogie ?? 30;
                TotalBogiesCount = CurrentTrain.ClassDistribution?.Executive?.BogiesCount ?? 1;
                BaseFare = Fare > 0 ? Fare : (defaultSchedulingFare * 1.70m);
            }

            if (string.IsNullOrEmpty(bogie))
            {
                CurrentBogie = $"{prefix}1";
            }
            else
            {
                CurrentBogie = bogie;
            }

            // ?? FIXED: Strict Date Range Check for MongoDB Driver
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                DateTime startOfDay = parsedDate.Date;
                DateTime endOfDay = startOfDay.AddDays(1);

                var activeBookings = _bookingCollection.Find(b =>
                    b.TrainId == CurrentTrain.Id &&
                    b.TravelDate >= startOfDay &&
                    b.TravelDate < endOfDay &&
                    b.SelectedClass == @class
                ).ToList();

                foreach (var booking in activeBookings)
                {
                    if (booking.BookedSeats != null)
                    {
                        foreach (var seat in booking.BookedSeats)
                        {
                            // Agar seat layout current bogie context "B1-" se match karti hai tabhi load karein
                            if (seat.StartsWith(CurrentBogie + "-"))
                            {
                                ExistingBookedSeats.Add(seat);
                            }
                        }
                    }
                }
            }

            return Page();
        }
    }
}