using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Safar.Models
{
    [BsonIgnoreExtraElements]
    public class Schedule
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("TrainId")]
        public string TrainId { get; set; }

        [BsonElement("TrainName")]
        public string TrainName { get; set; }

        [BsonElement("SourceStation")]
        public string SourceStation { get; set; }

        [BsonElement("DestinationStation")]
        public string DestinationStation { get; set; }

        [BsonElement("OperatingDays")]
        public List<string> OperatingDays { get; set; } = new List<string>();

        [BsonElement("RouteStops")]
        public List<StationStop> RouteStops { get; set; } = new List<StationStop>();

        [BsonElement("LastUpdated")]
        public DateTime LastUpdated { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class StationStop
    {
        [BsonElement("SequenceOrder")]
        public int SequenceOrder { get; set; }

        [BsonElement("StationName")]
        public string StationName { get; set; }

        [BsonElement("ArrivalTime")]
        public string ArrivalTime { get; set; }

        [BsonElement("DepartureTime")]
        public string DepartureTime { get; set; }

        [BsonElement("PriceFromSource")]
        public double PriceFromSource { get; set; }
    }
}