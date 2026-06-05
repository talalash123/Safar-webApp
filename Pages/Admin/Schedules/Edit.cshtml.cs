using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Safar.Pages.Admin.Schedules
{
    public class EditModel : PageModel
    {
        private readonly IMongoDatabase _database;

        public EditModel(IMongoDatabase database)
        {
            _database = database;
        }

        [BindProperty]
        public string TrainId { get; set; }

        [BindProperty]
        public string TrainName { get; set; }

        [BindProperty]
        public string SourceInputStation { get; set; }

        [BindProperty]
        public string DestinationInputStation { get; set; }

        [BindProperty]
        public string[] SelectedDays { get; set; }

        public List<string> CurrentOperatingDays { get; set; } = new List<string>();

        // For dynamic UI binding fallback mapping retention
        public string ExistingSourceDepartureTime { get; set; } = "08:00";
        public string ExistingDestArrivalTime { get; set; } = "21:00";
        public double ExistingDestTotalPrice { get; set; } = 0.0;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToPage("./Index");

            try
            {
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");
                if (ObjectId.TryParse(id, out ObjectId objId))
                {
                    var train = await trainCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", objId)).FirstOrDefaultAsync();
                    if (train == null) return RedirectToPage("./Index");

                    TrainId = id;
                    TrainName = train.Contains("LocomotiveEngineProfile") ? train["LocomotiveEngineProfile"].ToString() :
                                 (train.Contains("Name") ? train["Name"].ToString() : "Connected Engine");

                    // 1. Pehle Master Fleet Registry fields se saved data uthayein (No hardcoding!)
                    SourceInputStation = train.Contains("DefaultSource") ? train["DefaultSource"].ToString() : "Islamabad";
                    DestinationInputStation = train.Contains("DefaultDestination") ? train["DefaultDestination"].ToString() : "Karachi";

                    // 2. Agar RouteStops structural document array pehle se bani hui hai to actual values populate karein
                    if (train.Contains("RouteStops") && train["RouteStops"].IsBsonArray && train["RouteStops"].AsBsonArray.Count > 0)
                    {
                        var arr = train["RouteStops"].AsBsonArray;

                        var firstStop = arr[0].AsBsonDocument;
                        SourceInputStation = firstStop.Contains("StationName") ? firstStop["StationName"].ToString() : SourceInputStation;
                        ExistingSourceDepartureTime = firstStop.Contains("DepartureTime") ? firstStop["DepartureTime"].ToString() : "08:00";

                        var lastStop = arr[arr.Count - 1].AsBsonDocument;
                        DestinationInputStation = lastStop.Contains("StationName") ? lastStop["StationName"].ToString() : DestinationInputStation;
                        ExistingDestArrivalTime = lastStop.Contains("ArrivalTime") ? lastStop["ArrivalTime"].ToString() : "21:00";

                        if (lastStop.Contains("PriceFromSource"))
                        {
                            double.TryParse(lastStop["PriceFromSource"].ToString(), out double p);
                            ExistingDestTotalPrice = p;
                        }
                    }

                    if (train.Contains("OperatingDays") && train["OperatingDays"].IsBsonArray)
                    {
                        CurrentOperatingDays = train["OperatingDays"].AsBsonArray.Select(x => x.ToString()).ToList();
                    }
                }
            }
            catch (Exception) { }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(TrainId)) return Page();

            try
            {
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");
                if (ObjectId.TryParse(TrainId, out ObjectId objId))
                {
                    var daysArray = new BsonArray();
                    if (SelectedDays != null)
                    {
                        foreach (var d in SelectedDays) daysArray.Add(d);
                    }

                    var routeStopsArray = new BsonArray();

                    // 1. Initial Source Base Configuration Node
                    string srcDeparture = Request.Form["SourceDepartureTime"].ToString();
                    routeStopsArray.Add(new BsonDocument
                    {
                        { "SequenceOrder", 1 },
                        { "StationName", SourceInputStation },
                        { "ArrivalTime", "00:00" },
                        { "DepartureTime", !string.IsNullOrEmpty(srcDeparture) ? srcDeparture : "08:00" },
                        { "PriceFromSource", 0.0 }
                    });

                    // 2. Loop Dynamic Transit Midpoints Setup
                    var formKeys = Request.Form.Keys.Where(k => k.StartsWith("RouteStops[") && k.EndsWith("].StationName")).ToList();
                    int currentSeq = 2;

                    foreach (var key in formKeys)
                    {
                        string indexStr = key.Replace("RouteStops[", "").Replace("].StationName", "");
                        if (int.TryParse(indexStr, out int idx))
                        {
                            string stName = Request.Form[$"RouteStops[{idx}].StationName"].ToString();
                            if (!string.IsNullOrEmpty(stName))
                            {
                                string arrTime = Request.Form[$"RouteStops[{idx}].ArrivalTime"].ToString();
                                string depTime = Request.Form[$"RouteStops[{idx}].DepartureTime"].ToString();
                                string priceStr = Request.Form[$"RouteStops[{idx}].PriceFromSource"].ToString();
                                double.TryParse(priceStr, out double stopPrice);

                                routeStopsArray.Add(new BsonDocument
                                {
                                    { "SequenceOrder", currentSeq },
                                    { "StationName", stName },
                                    { "ArrivalTime", !string.IsNullOrEmpty(arrTime) ? arrTime : "12:00" },
                                    { "DepartureTime", !string.IsNullOrEmpty(depTime) ? depTime : "12:30" },
                                    { "PriceFromSource", stopPrice }
                                });
                                currentSeq++;
                            }
                        }
                    }

                    // 3. Final Bound Target Node Registration
                    string destArrival = Request.Form["DestArrivalTime"].ToString();
                    string destPriceStr = Request.Form["DestTotalPrice"].ToString();
                    double.TryParse(destPriceStr, out double destPrice);

                    routeStopsArray.Add(new BsonDocument
                    {
                        { "SequenceOrder", currentSeq },
                        { "StationName", DestinationInputStation },
                        { "ArrivalTime", !string.IsNullOrEmpty(destArrival) ? destArrival : "21:00" },
                        { "DepartureTime", "00:00" },
                        { "PriceFromSource", destPrice }
                    });

                    // Update structural profile fields inside the active document layout engine (Syncing Master Fields too)
                    var updateDefinition = Builders<BsonDocument>.Update
                        .Set("DefaultSource", SourceInputStation)
                        .Set("DefaultDestination", DestinationInputStation)
                        .Set("OperatingDays", daysArray)
                        .Set("RouteStops", routeStopsArray);

                    await trainCollection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", objId), updateDefinition);
                }
            }
            catch (Exception) { }

            return RedirectToPage("./Index");
        }
    }
}