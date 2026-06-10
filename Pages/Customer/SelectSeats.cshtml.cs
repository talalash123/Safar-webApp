using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using SafarWebApp.Services; // 🧠 ML Services namespace
using System;
using System.Collections.Generic;
using System.Linq;

namespace Safar.Pages.Customer
{
    public class SelectSeatsModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;
        private readonly IMongoCollection<Booking> _bookingCollection;

        // 🧠 1. Declare the AI Seating Service
        private readonly SeatingArrangementService _seatingService;

        public Train CurrentTrain { get; set; } = new Train();

        // 🛠️ Bind the Class and Fare so we can use them in the HTML
        [BindProperty(SupportsGet = true)]
        public string Class { get; set; }

        // 🧠 Alias so Razor views can use Model.SelectedClass
        public string SelectedClass => Class;

        public string TravelDateString { get; set; }

        public int TotalSeatsCount { get; set; }
        public int TotalBogiesCount { get; set; } = 1;
        public decimal BaseFare { get; set; }
        public string DisplayTrainCode { get; set; }

        // 🧠 Output variable for the frontend to highlight
        public string RecommendedSeatId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Src { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Dest { get; set; }

        // 🛠️ Bind the AI Fare from the URL
        [BindProperty(SupportsGet = true)]
        public decimal Fare { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CurrentBogie { get; set; }

        public List<string> ExistingBookedSeats { get; set; } = new List<string>();

        // 🧠 Real Demographics and Passenger Info Bound Properties
        [BindProperty(SupportsGet = true)]
        public int PassengerCount { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string LeadName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string LeadPhone { get; set; }

        [BindProperty(SupportsGet = true)]
        public string LeadCNIC { get; set; }

        [BindProperty(SupportsGet = true)]
        public int LeadAge { get; set; } = 30;

        [BindProperty(SupportsGet = true)]
        public string LeadGender { get; set; } = "Male";

        [BindProperty(SupportsGet = true)]
        public bool IsWithFamily { get; set; }

        [BindProperty(SupportsGet = true)]
        public string PassengerManifest { get; set; }

        // 🧠 2. Inject BOTH MongoDB and the AI Seating Service
        public SelectSeatsModel(IMongoDatabase database, SeatingArrangementService seatingService)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _bookingCollection = database.GetCollection<Booking>("Bookings");
            _seatingService = seatingService;
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

            Class = @class;
            TravelDateString = date;

            // Safe reflection check backup configuration
            DisplayTrainCode = !string.IsNullOrEmpty(CurrentTrain.TrainId)
                ? CurrentTrain.TrainId
                : (CurrentTrain.Id != null && CurrentTrain.Id.Length > 5
                    ? CurrentTrain.Id.Substring(CurrentTrain.Id.Length - 5).ToUpper()
                    : "T-REG");

            // 🛠️ Use the AI Fare from the URL if it exists, otherwise fallback to 1500
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

            // ==========================================
            // 🧠 AI DATA PREPARATION LISTS
            // ==========================================
            List<BookedSeatInfo> currentlyBookedDemographics = new List<BookedSeatInfo>();
            List<string> allPossibleSeats = new List<string>();

            // Build a quick list of all possible seats in this bogie (e.g., E1-S1 to E1-S72)
            for (int i = 1; i <= TotalSeatsCount; i++)
            {
                allPossibleSeats.Add($"{CurrentBogie}-S{i}");
            }

            // FIXED: Strict Date Range Check for MongoDB Driver
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

                                // 🧠 Pass the actual passenger data to the AI context list
                                currentlyBookedDemographics.Add(new BookedSeatInfo
                                {
                                    SeatNumber = seat,
                                    PassengerAge = booking.CustomerDetails?.Age ?? 30, // Fallback to 30 if null
                                    PassengerGender = booking.CustomerDetails?.Gender ?? "Male",
                                    IsFamilyOccupied = booking.CustomerDetails?.IsWithFamily ?? false
                                });
                            }
                        }
                    }
                }
            }

            // ==========================================
            // 🧠 APPLY MACHINE LEARNING SEATING LOGIC
            // ==========================================

            // 1. Calculate the final list of empty seats
            var availableEmptySeats = allPossibleSeats.Except(ExistingBookedSeats).ToList();

            // 2. Build passenger profile using real details entered by user in the PassengerInfo step
            var realCurrentUser = new PassengerProfile
            {
                Name = LeadName ?? "Lead Passenger",
                Age = LeadAge,
                Gender = LeadGender ?? "Male",
                IsWithFamily = IsWithFamily
            };

            // 3. Call the AI Engine!
            RecommendedSeatId = _seatingService.SuggestBestSeat(realCurrentUser, availableEmptySeats, currentlyBookedDemographics);

            return Page();
        }
    }
}