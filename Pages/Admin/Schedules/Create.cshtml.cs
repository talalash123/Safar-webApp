using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;

namespace Safar.Pages.Admin.Schedules
{
    public class CreateModel : PageModel
    {
        private readonly IMongoCollection<Schedule> _scheduleCollection;
        private readonly IMongoCollection<Train> _trainCollection;

        [BindProperty]
        public Schedule MasterSchedule { get; set; } = new Schedule();

        public Train TargetTrain { get; set; } = new Train();

        public CreateModel(IMongoDatabase database)
        {
            _scheduleCollection = database.GetCollection<Schedule>("Schedules");
            _trainCollection = database.GetCollection<Train>("Trains");
        }

        public IActionResult OnGet(string trainId)
        {
            if (string.IsNullOrEmpty(trainId))
            {
                return RedirectToPage("/Admin/Schedules/Index");
            }

            TargetTrain = _trainCollection.Find(t => t.TrainId == trainId).FirstOrDefault();

            if (TargetTrain == null)
            {
                return RedirectToPage("/Admin/Schedules/Index");
            }

            return Page();
        }

        public IActionResult OnPost(string sourceDepartureTime, string destinationArrivalTime)
        {
            // Re-fetch configuration context safely in case parameters fail validation blocks
            TargetTrain = _trainCollection.Find(t => t.TrainId == MasterSchedule.TrainId).FirstOrDefault();

            if (TargetTrain == null)
            {
                return RedirectToPage("/Admin/Schedules/Index");
            }

            if (string.IsNullOrEmpty(sourceDepartureTime) || string.IsNullOrEmpty(destinationArrivalTime) || MasterSchedule.BasePrice <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please fill departure, arrival timings and global ticket values properly.");
                return Page();
            }

            try
            {
                // Create a temporary operational track sequence list
                List<Stop> CompiledTransitLineNodes = new List<Stop>();

                // Node 1: Inject Source Station Endpoint Element at Index 0
                CompiledTransitLineNodes.Add(new Stop
                {
                    StationName = MasterSchedule.SourceStation,
                    ArrivalTime = sourceDepartureTime,
                    TicketPriceFromSource = 0 // Starting vertex is free
                });

                // Node 2: Dynamic Append Mid-Way stations provided by Admin sequence
                if (MasterSchedule.Stops != null && MasterSchedule.Stops.Count > 0)
                {
                    int totalStopsCount = MasterSchedule.Stops.Count;
                    // Divide budget price seamlessly across segments
                    int calculatedSegmentPrice = MasterSchedule.BasePrice / (totalStopsCount + 1);

                    for (int i = 0; i < totalStopsCount; i++)
                    {
                        MasterSchedule.Stops[i].TicketPriceFromSource = calculatedSegmentPrice * (i + 1);
                        CompiledTransitLineNodes.Add(MasterSchedule.Stops[i]);
                    }
                }

                // Node 3: Append Final Destination Base Line Terminal Node at the end
                CompiledTransitLineNodes.Add(new Stop
                {
                    StationName = MasterSchedule.DestinationStation,
                    ArrivalTime = destinationArrivalTime,
                    TicketPriceFromSource = MasterSchedule.BasePrice
                });

                // Assign the compiled robust array directly to master scheme model pipeline
                MasterSchedule.Stops = CompiledTransitLineNodes;

                // Push clean compiled stream element block to MongoDB instance cluster
                _scheduleCollection.InsertOne(MasterSchedule);

                return RedirectToPage("/Admin/Schedules/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Transactional database failure: {ex.Message}");
                return Page();
            }
        }
    }
}