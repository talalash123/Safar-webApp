using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;

namespace Safar.Pages.Customer
{
    public class SearchResultsModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;

        public List<Train> AvailableTrains { get; set; } = new List<Train>();

        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime TravelDate { get; set; }

        public SearchResultsModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public IActionResult OnGet(string source, string destination, string travelDate)
        {
            // Agar koi user direct URL hit krne ki koshish kry to wapis safety crash filter active ho
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            {
                return RedirectToPage("/Customer/Index");
            }

            Source = source;
            Destination = destination;

            if (DateTime.TryParse(travelDate, out DateTime parsedDate))
            {
                TravelDate = parsedDate;
            }
            else
            {
                TravelDate = DateTime.Now;
            }

            // MongoDB filter to extract matching route vectors that are active inside deployment registry
            var filter = Builders<Train>.Filter.And(
                Builders<Train>.Filter.Eq(t => t.DefaultSource, source),
                Builders<Train>.Filter.Eq(t => t.DefaultDestination, destination),
                Builders<Train>.Filter.Eq(t => t.Status, "Active")
            );

            AvailableTrains = _trainCollection.Find(filter).ToList();

            return Page();
        }
    }
}