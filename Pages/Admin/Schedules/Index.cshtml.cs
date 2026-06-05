using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Safar.Pages.Admin.Schedules
{
    public class IndexModel : PageModel
    {
        private readonly IMongoDatabase _database;

        public IndexModel(IMongoDatabase database)
        {
            _database = database;
        }

        // BsonDocument array use kar rahe hain taake model mapping crash ya model missing errors se jaan chote!
        public List<BsonDocument> MasterTrainFleet { get; set; } = new List<BsonDocument>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Apke database se direct "Trains" collection load ho rahi hai
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var filter = Builders<BsonDocument>.Filter.Regex("LocomotiveEngineProfile", new BsonRegularExpression(SearchTerm, "i"));
                    MasterTrainFleet = await trainCollection.Find(filter).ToListAsync();
                }
                else
                {
                    MasterTrainFleet = await trainCollection.Find(_ => true).ToListAsync();
                }
            }
            catch (Exception)
            {
                MasterTrainFleet = new List<BsonDocument>();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage();

            try
            {
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");
                if (ObjectId.TryParse(id, out ObjectId objId))
                {
                    await trainCollection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", objId));
                }
            }
            catch (Exception) { }

            return RedirectToPage();
        }
    }
}