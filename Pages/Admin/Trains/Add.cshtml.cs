using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;

namespace Safar.Pages.Admin.Trains
{
    public class AddModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;

        [BindProperty]
        public Train NewTrain { get; set; } = new Train();

        // ?? Class Matrix Dynamic ViewBindings
        [BindProperty]
        public int EconomyBogies { get; set; }
        [BindProperty]
        public int EconomySeatsPerBogie { get; set; } = 72;

        [BindProperty]
        public int BusinessBogies { get; set; }
        [BindProperty]
        public int BusinessSeatsPerBogie { get; set; } = 48;

        [BindProperty]
        public int ExecutiveBogies { get; set; }
        [BindProperty]
        public int ExecutiveSeatsPerBogie { get; set; } = 30;

        public AddModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public void OnGet()
        {
            // Clean states mapping - Koi defaults backend se input areas mein nahi jayengi
            NewTrain = new Train
            {
                Status = "Active",
                DefaultSource = "",
                DefaultDestination = ""
            };
        }

        public IActionResult OnPost()
        {
            // Manual validation check for required core properties to prevent pipeline chokes
            if (string.IsNullOrEmpty(NewTrain.TrainId) || string.IsNullOrEmpty(NewTrain.Name))
            {
                ModelState.AddModelError(string.Empty, "Train Code and Locomotive Model Name are strictly required.");
                return Page();
            }

            try
            {
                // Status failback guarantee configuration
                if (string.IsNullOrEmpty(NewTrain.Status))
                {
                    NewTrain.Status = "Active";
                }

                // ?? Injecting the Dynamic Class Breakdown Layer into our verified Train entity
                NewTrain.ClassDistribution = new ClassDistributionConfig
                {
                    Economy = new ClassMetrics { BogiesCount = EconomyBogies, SeatsPerBogie = EconomySeatsPerBogie },
                    Business = new ClassMetrics { BogiesCount = BusinessBogies, SeatsPerBogie = BusinessSeatsPerBogie },
                    Executive = new ClassMetrics { BogiesCount = ExecutiveBogies, SeatsPerBogie = ExecutiveSeatsPerBogie }
                };

                // Synchronous MongoDB cluster ingestion pipeline execution
                _trainCollection.InsertOne(NewTrain);

                // Absolute Redirect back onto fleet verification list ledger dashboard
                return RedirectToPage("/Admin/Trains/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Cluster transmission error: {ex.Message}");
                return Page();
            }
        }
    }
}