using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;

namespace Safar.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;
        private readonly IMongoCollection<Schedule> _scheduleCollection;

        public long TotalTrainsCount { get; set; }
        public long ActiveTrainsCount { get; set; }
        public long TotalSchedulesCount { get; set; }

        public DashboardModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
            _scheduleCollection = database.GetCollection<Schedule>("Schedules");
        }

        public void OnGet()
        {
            TotalTrainsCount = _trainCollection.CountDocuments(_ => true);
            ActiveTrainsCount = _trainCollection.CountDocuments(t => t.Status == "Active");
            TotalSchedulesCount = _scheduleCollection.CountDocuments(_ => true);
        }
    }
}