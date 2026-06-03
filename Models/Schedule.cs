using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Safar.Models
{
    public class Schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TrainId { get; set; } = string.Empty;
        public string SourceStation { get; set; } = string.Empty;
        public string DestinationStation { get; set; } = string.Empty;
        public int BasePrice { get; set; }

        public List<Stop> Stops { get; set; } = new List<Stop>();
    }

    public class Stop
    {
        public string StationName { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public int TicketPriceFromSource { get; set; } // Yeh backend par auto-calculate hoga
    }
}