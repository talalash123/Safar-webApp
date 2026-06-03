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