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

        public AddModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public void OnGet()
        {
            // Default active set karein jab page load ho
            NewTrain = new Train { Status = "Active" };
        }

        public IActionResult OnPost()
        {
            // Modifying validation pipeline check: 
            // Agar default ya automatic properties (jaise Id) ki wajah se state fail ho rhi hai, 
            // to hum manual check karenge taake system choke na ho.
            if (string.IsNullOrEmpty(NewTrain.TrainId) || string.IsNullOrEmpty(NewTrain.Name))
            {
                ModelState.AddModelError(string.Empty, "Train Code and Locomotive Model Name are strictly required.");
                return Page();
            }

            try
            {
                // Force check: Agar status field bypass hui ho to safe assignment karein
                if (string.IsNullOrEmpty(NewTrain.Status))
                {
                    NewTrain.Status = "Active";
                }

                // Core MongoDB Ingestion
                _trainCollection.InsertOne(NewTrain);

                // Absolute Redirect: Record add hote hi direct Fleet Management ledger table par jump karein
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