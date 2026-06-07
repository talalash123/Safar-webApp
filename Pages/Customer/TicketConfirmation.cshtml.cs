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

        // Custom clean container to prevent any mapping drops from base model definitions
        public Booking TargetTicket { get; set; } = new Booking();

        public TicketConfirmationModel(IMongoDatabase database)
        {
            // Direct BsonDocument collection mapping creates an ironclad bridge bypass
            _bookingsRawCollection = database.GetCollection<BsonDocument>("Bookings");
        }

        public IActionResult OnGet(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return RedirectToPage("/Customer/Index");
            }

            BsonDocument rawDoc = null;

            // 1. Try finding by ObjectId string first
            if (ticketId.Length == 24)
            {
                if (ObjectId.TryParse(ticketId, out ObjectId objId))
                {
                    rawDoc = _bookingsRawCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", objId)).FirstOrDefault();
                }
            }

            // 2. Fallback: If not found by ObjectId, query via tracking TicketNumber/PNR string
            if (rawDoc == null)
            {
                rawDoc = _bookingsRawCollection.Find(Builders<BsonDocument>.Filter.Eq("TicketNumber", ticketId)).FirstOrDefault();
            }

            if (rawDoc == null)
            {
                return RedirectToPage("/Customer/Index");
            }

            // 3. SECURE MANUAL MAPPER PIPELINE: 
            // Extracting values straight from BSON streams to ensure 0 value drops never occur
            TargetTicket.Id = rawDoc.Contains("_id") ? rawDoc["_id"].ToString() : "";
            TargetTicket.TicketNumber = rawDoc.Contains("TicketNumber") ? rawDoc["TicketNumber"].ToString() : "SAFAR-PENDING";
            TargetTicket.TrainName = rawDoc.Contains("TrainName") ? rawDoc["TrainName"].ToString() : "Safar Express";
            TargetTicket.SourceStation = rawDoc.Contains("SourceStation") ? rawDoc["SourceStation"].ToString() : "Origin";
            TargetTicket.DestinationStation = rawDoc.Contains("DestinationStation") ? rawDoc["DestinationStation"].ToString() : "Destination";
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

            // Seats List Allocation Map
            TargetTicket.BookedSeats = new List<string>();
            if (rawDoc.Contains("BookedSeats") && rawDoc["BookedSeats"].IsBsonArray)
            {
                foreach (var seat in rawDoc["BookedSeats"].AsBsonArray)
                {
                    TargetTicket.BookedSeats.Add(seat.ToString());
                }
            }

            // CRITICAL FARE CAPTURE MATRIX: BsonDecimal128 extraction logic applied safely
            if (rawDoc.Contains("TotalFare"))
            {
                var fareField = rawDoc["TotalFare"];

                if (fareField is BsonDecimal128 || fareField.BsonType == BsonType.Decimal128)
                {
                    TargetTicket.TotalFare = fareField.AsDecimal; // Direct native driver assignment
                }
                else if (fareField.IsNumeric)
                {
                    TargetTicket.TotalFare = Convert.ToDecimal(fareField.ToDouble());
                }
                else
                {
                    decimal.TryParse(fareField.ToString(), out decimal parsedFare);
                    TargetTicket.TotalFare = parsedFare;
                }
            }
            else if (rawDoc.Contains("totalFare")) // Caml-case check alternative fallback
            {
                var fareField = rawDoc["totalFare"];
                if (fareField.IsNumeric)
                {
                    TargetTicket.TotalFare = Convert.ToDecimal(fareField.ToDouble());
                }
            }

            return Page();
        }
    }
}