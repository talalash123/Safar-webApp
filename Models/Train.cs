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

        // BsonElement use karein taake error khatam ho jaye
        [BsonElement("SerialCode")]
        public string TrainId { get; set; }

        [BsonElement("LocomotiveEngineProfile")]
        public string Name { get; set; }

        // Isko object ya dynamic rakhein taake agar DB mein Int32 ho ya String, dono par crash na ho
        [BsonElement("VolumetricCapacity")]
 
        public string TotalCapacity { get; set; }

        [BsonElement("BogieSegments")]
        public string TotalBogies { get; set; }

        [BsonElement("StatusState")]
        public string Status { get; set; } = "Active";

        // Isko object ya BsonValue rakhein taake agar array bhi aaye toh crash na ho
        [BsonElement("OperatingDays")]
        public object OperatingDays { get; set; }

        [BsonElement("DefaultSource")]
        public string DefaultSource { get; set; } = "Islamabad";

        [BsonElement("DefaultDestination")]
        public string DefaultDestination { get; set; } = "Karachi";

        [BsonElement("RouteStops")]
        public List<BsonDocument> RouteStops { get; set; } = new List<BsonDocument>();
    }
}