using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System;

namespace Safar.Pages.Admin.Trains
{
    public class EditModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;

        [BindProperty]
        public Train SelectedTrain { get; set; } = new Train();

        // ?? Class Matrix Allocation Compartments holding layout modifications
        [BindProperty]
        public int EconomyBogies { get; set; }
        [BindProperty]
        public int EconomySeatsPerBogie { get; set; }

        [BindProperty]
        public int BusinessBogies { get; set; }
        [BindProperty]
        public int BusinessSeatsPerBogie { get; set; }

        [BindProperty]
        public int ExecutiveBogies { get; set; }
        [BindProperty]
        public int ExecutiveSeatsPerBogie { get; set; }

        public EditModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        // Page load hote waqt URL se Unique Object ID pkrna aur form fill krna
        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Admin/Trains/Index");
            }

            // Find specific train matching MongoDB structural Object ID
            SelectedTrain = _trainCollection.Find(t => t.Id == id).FirstOrDefault();

            if (SelectedTrain == null)
            {
                return RedirectToPage("/Admin/Trains/Index");
            }

            // Fallback guarantee initialization of nested objects if they don't exist in older documents
            if (SelectedTrain.ClassDistribution == null)
            {
                SelectedTrain.ClassDistribution = new ClassDistributionConfig();
            }

            // Feed existing database record parameters directly into UI properties to auto-populate form fields
            EconomyBogies = SelectedTrain.ClassDistribution.Economy?.BogiesCount ?? 0;
            EconomySeatsPerBogie = SelectedTrain.ClassDistribution.Economy?.SeatsPerBogie ?? 72;

            BusinessBogies = SelectedTrain.ClassDistribution.Business?.BogiesCount ?? 0;
            BusinessSeatsPerBogie = SelectedTrain.ClassDistribution.Business?.SeatsPerBogie ?? 48;

            ExecutiveBogies = SelectedTrain.ClassDistribution.Executive?.BogiesCount ?? 0;
            ExecutiveSeatsPerBogie = SelectedTrain.ClassDistribution.Executive?.SeatsPerBogie ?? 30;

            return Page();
        }

        // Changes submit krne ka execution pipeline
        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(SelectedTrain.TrainId) || string.IsNullOrEmpty(SelectedTrain.Name))
            {
                return Page();
            }

            try
            {
                // Inject updated interface allocation properties back into the nested configuration layer
                SelectedTrain.ClassDistribution = new ClassDistributionConfig
                {
                    Economy = new ClassMetrics { BogiesCount = EconomyBogies, SeatsPerBogie = EconomySeatsPerBogie },
                    Business = new ClassMetrics { BogiesCount = BusinessBogies, SeatsPerBogie = BusinessSeatsPerBogie },
                    Executive = new ClassMetrics { BogiesCount = ExecutiveBogies, SeatsPerBogie = ExecutiveSeatsPerBogie }
                };

                // Update implementation targeting the specific unique key document
                var filter = Builders<Train>.Filter.Eq(t => t.Id, SelectedTrain.Id);

                // Pure document object block ko replace/update karna
                _trainCollection.ReplaceOne(filter, SelectedTrain);

                // Update hotay hi return safely back to management ledger
                return RedirectToPage("/Admin/Trains/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update cluster entity: {ex.Message}");
                return Page();
            }
        }
    }
}