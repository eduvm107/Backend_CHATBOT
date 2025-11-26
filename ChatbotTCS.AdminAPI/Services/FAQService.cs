using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio dedicado para gestionar FAQs
    /// </summary>
    public class FAQService
    {
        private readonly IMongoCollection<FAQ> _faqsCollection;
        private readonly ILogger<FAQService> _logger;

        public FAQService(MongoDBService mongoDBService, ILogger<FAQService> logger)
        {
            _logger = logger;
            _faqsCollection = mongoDBService.GetCollection<FAQ>("faqs");
        }

        /// <summary>
        /// Obtiene todas las FAQs
        /// </summary>
        public async Task<List<FAQ>> GetAllAsync()
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
        /// Obtiene una FAQ por ID
        /// </summary>
        public async Task<FAQ?> GetByIdAsync(string id)
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
        /// Crea una nueva FAQ
        /// </summary>
        public async Task CreateAsync(FAQ faq)
        {
            try
            {
                _logger.LogInformation("Creando nueva FAQ: {Pregunta}", faq.Pregunta);

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
        public async Task<bool> UpdateAsync(string id, FAQ faq)
        {
            try
            {
                _logger.LogInformation("Actualizando FAQ con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                faq.FechaActualizacion = DateTime.UtcNow;
                faq.Id = id;

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
        /// Elimina una FAQ
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
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
        /// Busca FAQs usando regex (sin necesidad de índice de texto)
        /// </summary>
        public async Task<List<FAQ>> SearchAsync(string query)
        {
            try
            {
                _logger.LogInformation("Buscando FAQs con query: {Query}", query);

                if (string.IsNullOrWhiteSpace(query))
                {
                    return await GetAllAsync();
                }

                // Búsqueda case-insensitive usando regex (no requiere índice de texto)
                var filter = Builders<FAQ>.Filter.Or(
                    Builders<FAQ>.Filter.Regex(f => f.Pregunta, new BsonRegularExpression(query, "i")),
                    Builders<FAQ>.Filter.Regex(f => f.Respuesta, new BsonRegularExpression(query, "i")),
                    Builders<FAQ>.Filter.AnyIn(f => f.PalabrasClave, new[] { query.ToLower() })
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

        /// <summary>
        /// Busca FAQs relevantes para el contexto del chatbot
        /// Usa búsqueda por palabras clave con regex
        /// </summary>
        public async Task<List<FAQ>> BuscarRelevantesAsync(string pregunta)
        {
            try
            {
                _logger.LogInformation("Buscando FAQs relevantes para: {Pregunta}", pregunta);

                if (string.IsNullOrWhiteSpace(pregunta))
                {
                    return new List<FAQ>();
                }

                // Dividir la pregunta en palabras clave
                var palabrasClave = pregunta.ToLower()
                    .Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p.Length > 3) // Solo palabras de más de 3 caracteres
                    .ToList();

                if (!palabrasClave.Any())
                {
                    return new List<FAQ>();
                }

                // Construir filtro con regex para cada palabra clave
                var filters = new List<FilterDefinition<FAQ>>();

                foreach (var palabra in palabrasClave)
                {
                    var regex = new BsonRegularExpression(palabra, "i");

                    filters.Add(Builders<FAQ>.Filter.Or(
                        Builders<FAQ>.Filter.Regex(f => f.Pregunta, regex),
                        Builders<FAQ>.Filter.Regex(f => f.Respuesta, regex),
                        Builders<FAQ>.Filter.AnyElemMatch(f => f.PalabrasClave,
                            Builders<string>.Filter.Regex("$", regex))
                    ));
                }

                // Combinar todos los filtros con OR
                var combinedFilter = Builders<FAQ>.Filter.Or(filters);

                // Buscar y limitar a 3 resultados
                var results = await _faqsCollection
                    .Find(combinedFilter)
                    .Limit(3)
                    .ToListAsync();

                _logger.LogInformation("Se encontraron {Count} FAQs relevantes", results.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar FAQs relevantes");
                return new List<FAQ>();
            }
        }

        /// <summary>
        /// Obtiene el total de FAQs en la base de datos
        /// </summary>
        public async Task<long> CountAsync()
        {
            try
            {
                return await _faqsCollection.CountDocumentsAsync(FilterDefinition<FAQ>.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar FAQs");
                throw;
            }
        }
    }
}
