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

            // Conexión colección específica
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
            // Dimensiones esperadas del índice vectorial en MongoDB (según configuración del índice)
            const int ExpectedDimensions = 1024;

            if (queryVector == null)
            {
                throw new ArgumentNullException(nameof(queryVector), "El vector de consulta no puede ser null.");
            }

            if (queryVector.Length != ExpectedDimensions)
            {
                throw new ArgumentException(
                    $"El vector de consulta debe tener {ExpectedDimensions} dimensiones, pero tiene {queryVector.Length}.",
                    nameof(queryVector));
            }

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
