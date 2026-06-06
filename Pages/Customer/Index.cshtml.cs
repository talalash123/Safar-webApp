using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System.Collections.Generic;
using System.Linq;

namespace Safar.Pages.Customer
{

    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;

        public List<string> UniqueSources { get; set; } = new List<string>();
        public List<string> UniqueDestinations { get; set; } = new List<string>();

        // Naya List property featured display k liye
        public List<Train> FeaturedTrains { get; set; } = new List<Train>();

        public IndexModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public void OnGet()
        {
            // Database se sirf Active trains nikalna
            var allTrains = _trainCollection.Find(t => t.Status == "Active").ToList();

            // 1. Dropdown matching mechanisms (Distinct Stations)
            UniqueSources = allTrains
                .Where(t => !string.IsNullOrEmpty(t.DefaultSource))
                .Select(t => t.DefaultSource)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            UniqueDestinations = allTrains
                .Where(t => !string.IsNullOrEmpty(t.DefaultDestination))
                .Select(t => t.DefaultDestination)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // 2. Niche dynamic tickets display krne k liye top 3 active trains pick krna
            FeaturedTrains = allTrains.Take(3).ToList();
        }
    }
}