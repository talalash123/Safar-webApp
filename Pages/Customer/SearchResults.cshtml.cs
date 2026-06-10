using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using SafarWebApp.Services; // 🧠 ML Services namespace

namespace Safar.Pages.Customer
{
    // C# Model matching your EXACT MongoDB Trains schema
    [BsonIgnoreExtraElements]
    public class TrainCollectionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("SerialCode")]
        public string SerialCode { get; set; }

        [BsonElement("LocomotiveEngineProfile")]
        public string LocomotiveEngineProfile { get; set; }

        [BsonElement("VolumetricCapacity")]
        public string VolumetricCapacity { get; set; }

        [BsonElement("BogieSegments")]
        public string BogieSegments { get; set; }

        [BsonElement("StatusState")]
        public string StatusState { get; set; }

        [BsonElement("OperatingDays")]
        public List<string> OperatingDays { get; set; } = new List<string>();

        [BsonElement("RouteStops")]
        public List<TrainStationStop> RouteStops { get; set; } = new List<TrainStationStop>();
    }

    [BsonIgnoreExtraElements]
    public class TrainStationStop
    {
        [BsonElement("SequenceOrder")]
        public int SequenceOrder { get; set; }

        [BsonElement("StationName")]
        public string StationName { get; set; }

        [BsonElement("ArrivalTime")]
        public string ArrivalTime { get; set; }

        [BsonElement("DepartureTime")]
        public string DepartureTime { get; set; }

        [BsonElement("PriceFromSource")]
        public double PriceFromSource { get; set; }
    }

    public class MatchedTrainViewModel
    {
        public string Id { get; set; }
        public string TrainIdStr { get; set; }
        public string Name { get; set; }

        // 🧠 These will now hold the AI-Optimized Prices
        public decimal EconomyFare { get; set; }
        public decimal BusinessFare { get; set; }
        public decimal ExecutiveFare { get; set; }

        public string TotalBogies { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureTime { get; set; }
    }

    public class SearchResultsModel : PageModel
    {
        private readonly IMongoCollection<TrainCollectionModel> _trainCollection;

        // 🧠 1. Declare the AI Pricing Service
        private readonly DynamicPricingService _pricingService;

        [BindProperty(SupportsGet = true)]
        public string Source { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Destination { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Date { get; set; }

        public string SearchSource => Source ?? "";
        public string SearchDestination => Destination ?? "";
        public DateTime TravelDate { get; set; } = DateTime.Now;

        public List<MatchedTrainViewModel> AvailableSchedules { get; set; } = new List<MatchedTrainViewModel>();

        // 🧠 2. Inject BOTH MongoDB and the AI Pricing Service via Constructor
        public SearchResultsModel(IMongoDatabase database, DynamicPricingService pricingService)
        {
            _trainCollection = database.GetCollection<TrainCollectionModel>("Trains");
            _pricingService = pricingService;
        }

        private bool IsStationMatch(string userInput, string dbStation)
        {
            if (string.IsNullOrEmpty(userInput) || string.IsNullOrEmpty(dbStation)) return false;

            string u = userInput.Trim().ToLower();
            string d = dbStation.Trim().ToLower();

            if ((u.Contains("pindi") || u.Contains("rawalpindi")) && (d.Contains("pindi") || d.Contains("rawalpindi")))
                return true;

            return u.Contains(d) || d.Contains(u);
        }

        public void OnGet()
        {
            // ?? Premium Date Fallback Engine
            if (!string.IsNullOrEmpty(Date) && DateTime.TryParse(Date, out DateTime parsedDate))
            {
                TravelDate = parsedDate;
            }
            else
            {
                TravelDate = DateTime.Today;
                Date = TravelDate.ToString("yyyy-MM-dd");
            }

            var searchSource = Source?.Trim();
            var searchDest = Destination?.Trim();

            if (string.IsNullOrEmpty(searchSource) || string.IsNullOrEmpty(searchDest)) return;

            // 1. Direct Trains collection se saara data load karein
            var allTrains = _trainCollection.Find(t => t.StatusState == "Active").ToList();

            // 🧠 PREPARE AI VARIABLES
            int daysLeft = (TravelDate - DateTime.Today).Days;
            if (daysLeft < 0) daysLeft = 0; // Prevent negative days for past searches
            bool isEventDay = TravelDate.DayOfWeek == DayOfWeek.Saturday || TravelDate.DayOfWeek == DayOfWeek.Sunday;

            foreach (var train in allTrains)
            {
                // NULL CHECK GUARD
                if (train.RouteStops == null || train.RouteStops.Count == 0) continue;

                // 2. Dynamic Route Matrix Scanning
                var stops = train.RouteStops;
                var startStop = stops.FirstOrDefault(s => s != null && IsStationMatch(searchSource, s.StationName));
                var endStop = stops.FirstOrDefault(s => s != null && IsStationMatch(searchDest, s.StationName));

                // Agar user ka source aur destination is train ke route stops mein mil jata hai
                if (startStop != null && endStop != null && startStop.SequenceOrder < endStop.SequenceOrder)
                {
                    // 3. Real Price Calculation (Base Fare)
                    double baseTariff = endStop.PriceFromSource - startStop.PriceFromSource;
                    if (baseTariff <= 0) baseTariff = endStop.PriceFromSource;

                    // 🛠️ SAFETY NET: If the admin hasn't set individual station prices yet, 
                    // force the base tariff to the default 2500 so the AI doesn't break.
                    if (baseTariff <= 0)
                    {
                        baseTariff = 2500;
                    }

                    // ==========================================
                    // 🧠 4. APPLY MACHINE LEARNING DYNAMIC PRICING
                    // ==========================================
                    // Since live booking count isn't in TrainCollectionModel yet, 
                    // we simulate remaining seats (e.g., 40 seats left) for the AI model to process.
                    int simulatedRemainingSeats = 40;

                    decimal aiEconomyFare = _pricingService.PredictOptimalPrice(
                        (decimal)baseTariff,
                        simulatedRemainingSeats,
                        daysLeft,
                        isEventDay
                    );
                    // ==========================================

                    // 5. Verification & Model Population
                    AvailableSchedules.Add(new MatchedTrainViewModel
                    {
                        Id = train.Id ?? "",
                        TrainIdStr = train.SerialCode ?? "T1",
                        Name = train.LocomotiveEngineProfile ?? "Safar Express",
                        TotalBogies = train.BogieSegments ?? "10 Carriages",

                        // 🧠 Assigning the AI calculated prices (applying standard markups to the AI base)
                        EconomyFare = aiEconomyFare,
                        BusinessFare = Math.Round(aiEconomyFare * 1.40m, 2),  // 40% Markup on AI price
                        ExecutiveFare = Math.Round(aiEconomyFare * 1.70m, 2), // 70% Markup on AI price

                        DepartureTime = startStop.DepartureTime ?? "00:00 AM",
                        ArrivalTime = endStop.ArrivalTime ?? "00:00 PM"
                    });
                }
            }
        }
    }
}