using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Safar.Models
{
    [BsonIgnoreExtraElements]
    public class Train
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("SerialCode")]
        public string TrainId { get; set; }

        [BsonElement("LocomotiveEngineProfile")]
        public string Name { get; set; }

        [BsonElement("VolumetricCapacity")]
        public string TotalCapacity { get; set; }

        [BsonElement("BogieSegments")]
        public string TotalBogies { get; set; }

        [BsonElement("StatusState")]
        public string Status { get; set; } = "Active";

        [BsonElement("OperatingDays")]
        public object OperatingDays { get; set; }

        [BsonElement("DefaultSource")]
        public string DefaultSource { get; set; } = "Islamabad";

        [BsonElement("DefaultDestination")]
        public string DefaultDestination { get; set; } = "Karachi";

        [BsonElement("RouteStops")]
        public List<BsonDocument> RouteStops { get; set; } = new List<BsonDocument>();

        // 🚀 NEW: Class Partitioning Breakdown Schema Integration
        [BsonElement("ClassDistribution")]
        public ClassDistributionConfig ClassDistribution { get; set; } = new ClassDistributionConfig();
    }

    // 🗂️ Sub-Document Structure Maps for Class Distribution Ledger
    public class ClassDistributionConfig
    {
        [BsonElement("Economy")]
        public ClassMetrics Economy { get; set; } = new ClassMetrics { SeatsPerBogie = 72 };

        [BsonElement("Business")]
        public ClassMetrics Business { get; set; } = new ClassMetrics { SeatsPerBogie = 48 };

        [BsonElement("Executive")]
        public ClassMetrics Executive { get; set; } = new ClassMetrics { SeatsPerBogie = 30 };
    }

    public class ClassMetrics
    {
        [BsonElement("BogiesCount")]
        public int BogiesCount { get; set; } = 0;

        [BsonElement("SeatsPerBogie")]
        public int SeatsPerBogie { get; set; }
    }
}