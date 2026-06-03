using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Safar.Models
{
    public class Train
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string TrainId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int TotalBogies { get; set; }
        public string Status { get; set; } = "Active";

        // New Added Properties for Train Base Route
        public string DefaultSource { get; set; } = string.Empty;
        public string DefaultDestination { get; set; } = string.Empty;
    }
}