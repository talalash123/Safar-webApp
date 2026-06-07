using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Safar.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TicketNumber { get; set; } // Generated unique uppercase code (e.g., SAFAR-786X)

        public string TrainId { get; set; } // Train Code (e.g., T1)
        public string TrainName { get; set; }

        public string SourceStation { get; set; }
        public string DestinationStation { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime TravelDate { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerCNIC { get; set; }

        public string SelectedClass { get; set; } // Economy, Business, or Executive
        public List<string> BookedSeats { get; set; } = new List<string>(); // e.g., ["B1-S12", "B1-S13"]

        // 💎 THE ULTIMATE FIX: Directs MongoDB driver to cleanly serialize and map BsonDecimal128 without breaking execution pipelines
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalFare { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BookingTimestamp { get; set; } = DateTime.Now;

        public string PaymentStatus { get; set; } = "Paid"; // Default paid for simulation
    }
}