using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using Safar.Models;
using System;
using System.Collections.Generic;

namespace Safar.Pages.Customer
{
    public class TicketConfirmationModel : PageModel
    {
        private readonly IMongoCollection<BsonDocument> _bookingsRawCollection;

        public Booking TargetTicket { get; set; } = new Booking();

        public TicketConfirmationModel(IMongoDatabase database)
        {
            _bookingsRawCollection = database.GetCollection<BsonDocument>("Bookings");
        }

        public IActionResult OnGet(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return RedirectToPage("/Customer/Index");
            }

            BsonDocument rawDoc = null;

            if (ticketId.Length == 24)
            {
                if (ObjectId.TryParse(ticketId, out ObjectId objId))
                {
                    rawDoc = _bookingsRawCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", objId)).FirstOrDefault();
                }
            }

            if (rawDoc == null)
            {
                rawDoc = _bookingsRawCollection.Find(Builders<BsonDocument>.Filter.Eq("TicketNumber", ticketId)).FirstOrDefault();
            }

            if (rawDoc == null)
            {
                return RedirectToPage("/Customer/Index");
            }

            // ??? Secure String Field Assignments
            TargetTicket.Id = rawDoc.Contains("_id") ? rawDoc["_id"].ToString() : "";
            TargetTicket.TicketNumber = rawDoc.Contains("TicketNumber") ? rawDoc["TicketNumber"].ToString() : "SAFAR-PENDING";
            TargetTicket.TrainName = rawDoc.Contains("TrainName") ? rawDoc["TrainName"].ToString() : "Safar Express";

            TargetTicket.SourceStation = rawDoc.Contains("SelectedSource") ? rawDoc["SelectedSource"].ToString() :
                                         (rawDoc.Contains("SourceStation") ? rawDoc["SourceStation"].ToString() : "Rawalpindi");

            TargetTicket.DestinationStation = rawDoc.Contains("SelectedDestination") ? rawDoc["SelectedDestination"].ToString() :
                                              (rawDoc.Contains("DestinationStation") ? rawDoc["DestinationStation"].ToString() : "Gujranwala");

            TargetTicket.CustomerName = rawDoc.Contains("CustomerName") ? rawDoc["CustomerName"].ToString() : "Passenger";
            TargetTicket.CustomerPhone = rawDoc.Contains("CustomerPhone") ? rawDoc["CustomerPhone"].ToString() : "";
            TargetTicket.CustomerCNIC = rawDoc.Contains("CustomerCNIC") ? rawDoc["CustomerCNIC"].ToString() : "N/A";
            TargetTicket.SelectedClass = rawDoc.Contains("SelectedClass") ? rawDoc["SelectedClass"].ToString() : "Standard";

            // DateTime Parsing Guard
            if (rawDoc.Contains("TravelDate"))
            {
                if (rawDoc["TravelDate"].IsBsonDateTime)
                {
                    TargetTicket.TravelDate = rawDoc["TravelDate"].ToUniversalTime();
                }
                else if (DateTime.TryParse(rawDoc["TravelDate"].ToString(), out DateTime parsedDate))
                {
                    TargetTicket.TravelDate = parsedDate;
                }
            }

            // Seats List Array Extraction
            TargetTicket.BookedSeats = new List<string>();
            if (rawDoc.Contains("BookedSeats") && rawDoc["BookedSeats"].IsBsonArray)
            {
                foreach (var seat in rawDoc["BookedSeats"].AsBsonArray)
                {
                    TargetTicket.BookedSeats.Add(seat.ToString());
                }
            }

            // ?? BULLETPROOF FARE EXTRACTION MATRIX
            decimal extractedFare = 0;
            string keyToUse = rawDoc.Contains("TotalFare") ? "TotalFare" : (rawDoc.Contains("totalFare") ? "totalFare" : null);

            if (keyToUse != null)
            {
                var element = rawDoc[keyToUse];
                try
                {
                    if (element is BsonDecimal128 || element.BsonType == BsonType.Decimal128)
                    {
                        extractedFare = element.AsDecimal;
                    }
                    else if (element.IsNumeric)
                    {
                        extractedFare = Convert.ToDecimal(element.ToDouble());
                    }
                    else
                    {
                        decimal.TryParse(element.ToString(), out decimal parsed);
                        extractedFare = parsed;
                    }
                }
                catch
                {
                    decimal.TryParse(element.ToString(), out extractedFare);
                }
            }

            // Assign the absolute historical price without enforcing static calculations overrides
            TargetTicket.TotalFare = extractedFare;

            return Page();
        }
    }
}