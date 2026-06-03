using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;

namespace Safar.Pages.Admin.Schedules
{
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;
        private readonly IMongoCollection<Schedule> _scheduleCollection;

        public List<ScheduleCardViewModel> SchedulesMetadataDashboard { get; set; } = new List<ScheduleCardViewModel>();

        public IndexModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _scheduleCollection = database.GetCollection<Schedule>("Schedules");
        }

        public void OnGet()
        {
            var trains = _trainCollection.Find(_ => true).ToList();
            var schedules = _scheduleCollection.Find(_ => true).ToList();

            foreach (var train in trains)
            {
                // Core verification link using relational entity parameters mapping
                var matchingSchedule = schedules.FirstOrDefault(s => s.TrainId == train.TrainId);

                SchedulesMetadataDashboard.Add(new ScheduleCardViewModel
                {
                    TrainId = train.TrainId,
                    TrainName = train.Name,
                    Source = !string.IsNullOrEmpty(train.DefaultSource) ? train.DefaultSource : "Not Set",
                    Destination = !string.IsNullOrEmpty(train.DefaultDestination) ? train.DefaultDestination : "Not Set",
                    IsConfigured = matchingSchedule != null,
                    TotalStopsMapped = matchingSchedule?.Stops?.Count ?? 0,
                    StopsDetailedCollection = matchingSchedule?.Stops
                });
            }
        }
    }

    public class ScheduleCardViewModel
    {
        public string TrainId { get; set; }
        public string TrainName { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public bool IsConfigured { get; set; }
        public int TotalStopsMapped { get; set; }
        public List<Stop> StopsDetailedCollection { get; set; }
    }
}