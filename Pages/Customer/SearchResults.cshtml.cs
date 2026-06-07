using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Safar.Pages.Customer
{
    // C# Model matching your EXACT MongoDB Trains schema from screenshot
    [BsonIgnoreExtraElements]
    public class TrainCollectionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("SerialCode")]
        public string SerialCode { get; set; } // e.g., "T1", "T2"

        [BsonElement("LocomotiveEngineProfile")]
        public string LocomotiveEngineProfile { get; set; } // e.g., "IslamabadExpress"

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

        public SearchResultsModel(IMongoDatabase database)
        {
            // Direct tracking inside Trains since your records live there right now
            _trainCollection = database.GetCollection<TrainCollectionModel>("Trains");
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
            if (!string.IsNullOrEmpty(Date) && DateTime.TryParse(Date, out DateTime parsedDate))
            {
                TravelDate = parsedDate;
            }

            var searchSource = Source?.Trim();
            var searchDest = Destination?.Trim();

            if (string.IsNullOrEmpty(searchSource) || string.IsNullOrEmpty(searchDest)) return;

            // Selected day nikalna (e.g., "Sunday")
            string selectedDayOfWeek = TravelDate.ToString("DayOfWeek");

            // 1. Direct Trains collection se saara data load karein
            var allTrains = _trainCollection.Find(t => t.StatusState == "Active").ToList();

            foreach (var train in allTrains)
            {
                // NULL CHECK GUARD: Agar RouteStops hi khali hain toh agli train par chalien
                if (train.RouteStops == null || train.RouteStops.Count == 0)
                {
                    continue;
                }

                // 2. Dynamic Route Matrix Scanning (Intermediate stops checking)
                var stops = train.RouteStops;
                var startStop = stops.FirstOrDefault(s => s != null && IsStationMatch(searchSource, s.StationName));
                var endStop = stops.FirstOrDefault(s => s != null && IsStationMatch(searchDest, s.StationName));

                // Agar user ka source aur destination is train ke route stops mein mil jata hai
                if (startStop != null && endStop != null && startStop.SequenceOrder < endStop.SequenceOrder)
                {
                    // 3. Real Price Calculation: End stop fare minus Start stop fare
                    double baseTariff = endStop.PriceFromSource - startStop.PriceFromSource;

                    // Agar pehla hi station ho toh automatic endStop ki price lag jaye
                    if (baseTariff <= 0)
                    {
                        baseTariff = endStop.PriceFromSource;
                    }

                    decimal economyFare = (decimal)baseTariff;

                    // 4. Final Verification: Agar fares zero se barhi hain toh model mein add karein
                    AvailableSchedules.Add(new MatchedTrainViewModel
                    {
                        Id = train.Id ?? "",
                        TrainIdStr = train.SerialCode ?? "T1",
                        Name = train.LocomotiveEngineProfile ?? "Safar Express",
                        TotalBogies = train.BogieSegments ?? "10 Carriages",
                        EconomyFare = economyFare,
                        BusinessFare = economyFare * 1.40m,  // 40% Markup
                        ExecutiveFare = economyFare * 1.70m, // 70% Markup
                        DepartureTime = startStop.DepartureTime ?? "00:00 AM",
                        ArrivalTime = endStop.ArrivalTime ?? "00:00 PM"
                    });
                }
            }
        }
    }
}