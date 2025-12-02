using ChatbotTCS.AdminAPI.Models; // Asegúrate de tener tus modelos aquí
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    public class RagMongoService
    {
        private readonly IMongoCollection<KnowledgeBase> _collection;

        public RagMongoService(IConfiguration config)
        {
            // Leemos la misma conexión del equipo
            var connectionString = config["MongoDB:ConnectionString"];
            var databaseName = config["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            // Coneccion colección específica
            _collection = database.GetCollection<KnowledgeBase>("documentsEmbedding");
        }

        // Guardar conocimiento (Embeddings)
        public async Task AddKnowledgeAsync(KnowledgeBase kb)
        {
            await _collection.InsertOneAsync(kb);
        }

        // Buscar similitudes (Vector Search)
        public async Task<List<KnowledgeBase>> SearchAsync(double[] queryVector, int limit = 3)
        {
            // Nombre del índice en Atlas "vector_index"
            var indexName = "vector_index";

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$vectorSearch", new BsonDocument
                {
                    { "index", indexName },
                    { "path", "embedding" },
                    { "queryVector", new BsonArray(queryVector) },
                    { "numCandidates", 100 },
                    { "limit", limit }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "content", 1 },
                    { "category", 1 },
                    { "score", new BsonDocument("$meta", "vectorSearchScore") }
                })
            };

            return await _collection.Aggregate<KnowledgeBase>(pipeline).ToListAsync();
        }
    }
}