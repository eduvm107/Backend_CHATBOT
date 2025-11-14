using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para interactuar con MongoDB y gestionar FAQs
    /// </summary>
    public class MongoDBService
    {
        private readonly IMongoCollection<FAQ> _faqsCollection;
        private readonly ILogger<MongoDBService> _logger;

        /// <summary>
        /// Constructor del servicio MongoDB
        /// </summary>
        /// <param name="settings">Configuración de MongoDB</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public MongoDBService(IOptions<MongoDBSettings> settings, ILogger<MongoDBService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _faqsCollection = database.GetCollection<FAQ>("faqs");

                _logger.LogInformation("Conexión a MongoDB establecida exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las FAQs de la base de datos
        /// </summary>
        /// <returns>Lista de FAQs</returns>
        public async Task<List<FAQ>> GetAllFAQsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las FAQs");
                return await _faqsCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las FAQs");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una FAQ específica por su ID
        /// </summary>
        /// <param name="id">ID de la FAQ</param>
        /// <returns>FAQ encontrada o null</returns>
        public async Task<FAQ?> GetFAQByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo FAQ con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<FAQ>.Filter.Eq(f => f.Id, id);
                return await _faqsCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener FAQ con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva FAQ en la base de datos
        /// </summary>
        /// <param name="faq">FAQ a crear</param>
        public async Task CreateFAQAsync(FAQ faq)
        {
            try
            {
                _logger.LogInformation("Creando nueva FAQ: {Pregunta}", faq.Pregunta);

                // Establecer fechas de creación y actualización
                faq.FechaCreacion = DateTime.UtcNow;
                faq.FechaActualizacion = DateTime.UtcNow;

                await _faqsCollection.InsertOneAsync(faq);

                _logger.LogInformation("FAQ creada exitosamente con ID: {Id}", faq.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear FAQ");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una FAQ existente
        /// </summary>
        /// <param name="id">ID de la FAQ a actualizar</param>
        /// <param name="faq">Datos actualizados de la FAQ</param>
        /// <returns>True si se actualizó, False si no se encontró</returns>
        public async Task<bool> UpdateFAQAsync(string id, FAQ faq)
        {
            try
            {
                _logger.LogInformation("Actualizando FAQ con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                // Actualizar fecha de modificación
                faq.FechaActualizacion = DateTime.UtcNow;
                faq.Id = id; // Asegurar que el ID no cambie

                var filter = Builders<FAQ>.Filter.Eq(f => f.Id, id);
                var result = await _faqsCollection.ReplaceOneAsync(filter, faq);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("FAQ actualizada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró FAQ con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar FAQ con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una FAQ de la base de datos
        /// </summary>
        /// <param name="id">ID de la FAQ a eliminar</param>
        /// <returns>True si se eliminó, False si no se encontró</returns>
        public async Task<bool> DeleteFAQAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando FAQ con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<FAQ>.Filter.Eq(f => f.Id, id);
                var result = await _faqsCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("FAQ eliminada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró FAQ con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar FAQ con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Busca FAQs que coincidan con el texto de búsqueda
        /// </summary>
        /// <param name="query">Texto a buscar</param>
        /// <returns>Lista de FAQs que coinciden</returns>
        public async Task<List<FAQ>> SearchFAQsAsync(string query)
        {
            try
            {
                _logger.LogInformation("Buscando FAQs con query: {Query}", query);

                if (string.IsNullOrWhiteSpace(query))
                {
                    return await GetAllFAQsAsync();
                }

                // Búsqueda case-insensitive en pregunta, respuesta y palabras clave
                var filter = Builders<FAQ>.Filter.Or(
                    Builders<FAQ>.Filter.Regex(f => f.Pregunta, new BsonRegularExpression(query, "i")),
                    Builders<FAQ>.Filter.Regex(f => f.Respuesta, new BsonRegularExpression(query, "i")),
                    Builders<FAQ>.Filter.AnyIn(f => f.PalabrasClave, new[] { query })
                );

                var results = await _faqsCollection.Find(filter).ToListAsync();

                _logger.LogInformation("Se encontraron {Count} FAQs", results.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar FAQs con query: {Query}", query);
                throw;
            }
        }
    }
}
