using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;

namespace Safar.Pages.Admin.Trains
{
    public class IndexModel : PageModel
    {
        private readonly IMongoCollection<Train> _trainCollection;

        public List<Train> PakistanFleetRegistry { get; set; } = new List<Train>();

        public IndexModel(IMongoDatabase database)
        {
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public void OnGet()
        {
            // Database se saari trains load karke list mein daalna
            PakistanFleetRegistry = _trainCollection.Find(_ => true).ToList();
        }

        // Yeh handler execute hoga jab delete icon/button trigger hoga
        public IActionResult OnGetPurgeAssetNode(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                // MongoDB se directly string ID match karke document remove karna
                _trainCollection.DeleteOne(t => t.Id == id);
            }

            // Delete hone ke baad instant main list page par wapas le aana
            return RedirectToPage("/Admin/Trains/Index");
        }
    }
}