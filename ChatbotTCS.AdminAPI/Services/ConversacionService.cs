using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar conversaciones del chatbot
    /// </summary>
    public class ConversacionService
    {
        private readonly IMongoCollection<Conversacion> _conversacionesCollection;
        private readonly ILogger<ConversacionService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public ConversacionService(IOptions<MongoDBSettings> settings, ILogger<ConversacionService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _conversacionesCollection = database.GetCollection<Conversacion>("conversaciones");

                _logger.LogInformation("Conexión a colección conversaciones establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - conversaciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las conversaciones
        /// </summary>
        public async Task<List<Conversacion>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las conversaciones");
                return await _conversacionesCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una conversación por ID
        /// </summary>
        public async Task<Conversacion?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo conversación con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Id, id);
                return await _conversacionesCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversación con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva conversación
        /// </summary>
        public async Task CreateAsync(Conversacion conversacion)
        {
            try
            {
                _logger.LogInformation("Creando nueva conversación para usuario: {UsuarioId}", conversacion.UsuarioId);

                conversacion.FechaInicio = DateTime.UtcNow;
                conversacion.FechaUltimaMensaje = DateTime.UtcNow;
                await _conversacionesCollection.InsertOneAsync(conversacion);

                _logger.LogInformation("Conversación creada con ID: {Id}", conversacion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conversación");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una conversación existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, Conversacion conversacion)
        {
            try
            {
                _logger.LogInformation("Actualizando conversación con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                conversacion.FechaUltimaMensaje = DateTime.UtcNow;
                conversacion.Id = id;
                var filter = Builders<Conversacion>.Filter.Eq(c => c.Id, id);
                var result = await _conversacionesCollection.ReplaceOneAsync(filter, conversacion);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Conversación actualizada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró conversación con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar conversación con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una conversación
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando conversación con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Id, id);
                var result = await _conversacionesCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Conversación eliminada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró conversación con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar conversación con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene conversaciones por usuario
        /// </summary>
        public async Task<List<Conversacion>> GetByUsuarioIdAsync(string usuarioId)
        {
            try
            {
                _logger.LogInformation("Obteniendo conversaciones del usuario: {UsuarioId}", usuarioId);

                var filter = Builders<Conversacion>.Filter.Eq(c => c.UsuarioId, usuarioId);
                return await _conversacionesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones del usuario: {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene conversaciones activas
        /// </summary>
        public async Task<List<Conversacion>> GetActivasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo conversaciones activas");

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Activa, true);
                return await _conversacionesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones activas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene conversaciones resueltas
        /// </summary>
        public async Task<List<Conversacion>> GetResueltasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo conversaciones resueltas");

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Resuelto, true);
                return await _conversacionesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones resueltas");
                throw;
            }
        }

        /// <summary>
        /// Agrega un mensaje a una conversación existente
        /// </summary>
        public async Task<bool> AddMensajeAsync(string id, Mensaje mensaje)
        {
            try
            {
                _logger.LogInformation("Agregando mensaje a conversación: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Id, id);
                var update = Builders<Conversacion>.Update
                    .Push(c => c.Mensajes, mensaje)
                    .Set(c => c.FechaUltimaMensaje, DateTime.UtcNow);

                var result = await _conversacionesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Mensaje agregado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró conversación con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar mensaje a conversación: {Id}", id);
                throw;
            }
        }
    }
}
