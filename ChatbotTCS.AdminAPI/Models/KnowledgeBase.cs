using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    //  ignora cualquier otro dato extra inesperado
    [BsonIgnoreExtraElements]
    public class KnowledgeBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("embedding")]
        public double[] Embedding { get; set; }

       
        // Aquí se guardará el puntaje de similitud que se traiga de MongoDB
        [BsonElement("score")]
        public double? Score { get; set; }
    }
}