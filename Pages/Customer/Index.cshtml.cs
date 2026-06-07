using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Safar.Models;

namespace Safar.Pages.Customer
{
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Schedule> _scheduleCollection;

        public List<string> UniqueStations { get; set; } = new List<string>();

        public IndexModel(IMongoDatabase database)
        {
            _scheduleCollection = database.GetCollection<Schedule>("Schedules");
        }

        public void OnGet()
        {
            var allSchedules = _scheduleCollection.Find(s => true).ToList();
            var stationsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var schedule in allSchedules)
            {
                if (!string.IsNullOrEmpty(schedule.SourceStation))
                    stationsSet.Add(schedule.SourceStation.Trim());

                if (!string.IsNullOrEmpty(schedule.DestinationStation))
                    stationsSet.Add(schedule.DestinationStation.Trim());

                if (schedule.RouteStops != null)
                {
                    foreach (var stop in schedule.RouteStops)
                    {
                        if (!string.IsNullOrEmpty(stop.StationName))
                            stationsSet.Add(stop.StationName.Trim());
                    }
                }
            }

            // Fallback baseline values taake agar DB empty bhi ho toh system look kharab na ho
            if (stationsSet.Count == 0)
            {
                stationsSet.Add("Pindi");
                stationsSet.Add("Gujaranwala");
                stationsSet.Add("Lahore");
                stationsSet.Add("Islamabad");
            }

            UniqueStations = stationsSet.OrderBy(s => s).ToList();
        }
    }
}