using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Bson;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Safar.Pages.Admin.Schedules
{
    public class CreateModel : PageModel
    {
        private readonly IMongoDatabase _database;

        public CreateModel(IMongoDatabase database)
        {
            _database = database;
        }

        [BindProperty]
        public Schedule ScheduleData { get; set; } = new Schedule();

        [BindProperty]
        public string[] SelectedDays { get; set; }

        // Dropdown list items
        public List<SelectListItem> AvailableTrains { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            try
            {
                // Dynamic mapping directly using BsonDocument to avoid missing model class errors
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");
                var trainsInDb = await trainCollection.Find(_ => true).ToListAsync();

                foreach (var doc in trainsInDb)
                {
                    string id = doc.Contains("_id") ? doc["_id"].ToString() : "";
                    if (string.IsNullOrEmpty(id) && doc.Contains("Id")) id = doc["Id"].ToString();

                    string name = "";
                    if (doc.Contains("LocomotiveEngineProfile")) name = doc["LocomotiveEngineProfile"].ToString();
                    else if (doc.Contains("TrainName")) name = doc["TrainName"].ToString();
                    else if (doc.Contains("trainName")) name = doc["trainName"].ToString();

                    if (string.IsNullOrEmpty(name))
                    {
                        foreach (var element in doc.Elements)
                        {
                            if (element.Name != "_id" && element.Name != "Id" && element.Value.IsString)
                            {
                                name = element.Value.AsString;
                                break;
                            }
                        }
                    }

                    string serialCode = doc.Contains("SerialCode") ? doc["SerialCode"].ToString() : "";
                    if (string.IsNullOrEmpty(serialCode) && doc.Contains("serialCode")) serialCode = doc["serialCode"].ToString();

                    if (string.IsNullOrEmpty(name)) name = "Registered Locomotive";
                    string displayText = !string.IsNullOrEmpty(serialCode) ? $"{serialCode} - {name}" : name;

                    if (!string.IsNullOrEmpty(id))
                    {
                        AvailableTrains.Add(new SelectListItem
                        {
                            Value = id,
                            Text = displayText
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AvailableTrains.Add(new SelectListItem { Value = "error", Text = $"DB Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(ScheduleData.TrainId) || ScheduleData.TrainId == "error")
            {
                await OnGetAsync();
                return Page();
            }

            try
            {
                // Fetch Selected Train Profile from DB
                var trainCollection = _database.GetCollection<BsonDocument>("Trains");
                if (ObjectId.TryParse(ScheduleData.TrainId, out ObjectId filterId))
                {
                    var targetTrain = await trainCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", filterId)).FirstOrDefaultAsync();
                    if (targetTrain != null)
                    {
                        if (targetTrain.Contains("LocomotiveEngineProfile")) ScheduleData.TrainName = targetTrain["LocomotiveEngineProfile"].ToString();
                        else if (targetTrain.Contains("TrainName")) ScheduleData.TrainName = targetTrain["TrainName"].ToString();
                        else if (targetTrain.Contains("trainName")) ScheduleData.TrainName = targetTrain["trainName"].ToString();
                    }
                }

                // Setup default or selected track days
                if (SelectedDays != null && SelectedDays.Length > 0)
                {
                    ScheduleData.OperatingDays = SelectedDays.ToList();
                }
                else
                {
                    ScheduleData.OperatingDays = new List<string> { "Daily Track" };
                }

                // Build Clean Route Map List
                var cleanStops = new List<StationStop>();

                // Source Station Build
                string srcDeparture = Request.Form["ScheduleData.RouteStops[0].DepartureTime"].ToString();
                cleanStops.Add(new StationStop
                {
                    SequenceOrder = 1,
                    StationName = ScheduleData.SourceStation,
                    ArrivalTime = "00:00",
                    DepartureTime = !string.IsNullOrEmpty(srcDeparture) ? srcDeparture : "08:00",
                    PriceFromSource = 0
                });

                // Loop through intermediate items in payload
                var formKeys = Request.Form.Keys.Where(k => k.StartsWith("ScheduleData.RouteStops[") && k.EndsWith("].StationName")).ToList();
                int currentSeq = 2;

                foreach (var key in formKeys)
                {
                    string indexStr = key.Replace("ScheduleData.RouteStops[", "").Replace("].StationName", "");
                    if (int.TryParse(indexStr, out int idx) && idx > 0)
                    {
                        string stName = Request.Form[$"ScheduleData.RouteStops[{idx}].StationName"].ToString();
                        if (!string.IsNullOrEmpty(stName))
                        {
                            string rawArrival = Request.Form[$"ScheduleData.RouteStops[{idx}].ArrivalTime"].ToString();
                            string rawDeparture = Request.Form[$"ScheduleData.RouteStops[{idx}].DepartureTime"].ToString();
                            string rawPrice = Request.Form[$"ScheduleData.RouteStops[{idx}].PriceFromSource"].ToString();

                            double.TryParse(rawPrice, out double stopPrice);

                            cleanStops.Add(new StationStop
                            {
                                SequenceOrder = currentSeq,
                                StationName = stName,
                                ArrivalTime = !string.IsNullOrEmpty(rawArrival) ? rawArrival : "12:00",
                                DepartureTime = !string.IsNullOrEmpty(rawDeparture) ? rawDeparture : "12:30",
                                PriceFromSource = stopPrice
                            });
                            currentSeq++;
                        }
                    }
                }

                // Destination Hub Processing
                string destArrival = Request.Form["destArrivalTime"].ToString();
                string destPriceStr = Request.Form["destTotalPrice"].ToString();
                double.TryParse(destPriceStr, out double destPrice);

                cleanStops.Add(new StationStop
                {
                    SequenceOrder = currentSeq,
                    StationName = ScheduleData.DestinationStation,
                    ArrivalTime = !string.IsNullOrEmpty(destArrival) ? destArrival : "20:00",
                    DepartureTime = "00:00",
                    PriceFromSource = destPrice
                });

                ScheduleData.RouteStops = cleanStops;
                ScheduleData.LastUpdated = DateTime.UtcNow;
                ScheduleData.Id = null; // Forces MongoDB to register as fresh clean record

                var schedulesCollection = _database.GetCollection<Schedule>("Schedules");
                await schedulesCollection.InsertOneAsync(ScheduleData);

                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                await OnGetAsync();
                return Page();
            }
        }
    }
}