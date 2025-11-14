using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar mensajes automáticos
    /// </summary>
    public class MensajeAutomaticoService
    {
        private readonly IMongoCollection<MensajeAutomatico> _mensajesCollection;
        private readonly ILogger<MensajeAutomaticoService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public MensajeAutomaticoService(IOptions<MongoDBSettings> settings, ILogger<MensajeAutomaticoService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _mensajesCollection = database.GetCollection<MensajeAutomatico>("mensajesAutomaticos");

                _logger.LogInformation("Conexión a colección mensajesAutomaticos establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - mensajesAutomaticos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los mensajes automáticos
        /// </summary>
        public async Task<List<MensajeAutomatico>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los mensajes automáticos");
                return await _mensajesCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes automáticos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un mensaje automático por ID
        /// </summary>
        public async Task<MensajeAutomatico?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo mensaje automático con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<MensajeAutomatico>.Filter.Eq(m => m.Id, id);
                return await _mensajesCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensaje automático con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo mensaje automático
        /// </summary>
        public async Task CreateAsync(MensajeAutomatico mensaje)
        {
            try
            {
                _logger.LogInformation("Creando nuevo mensaje automático: {Titulo}", mensaje.Titulo);

                mensaje.FechaCreacion = DateTime.UtcNow;
                await _mensajesCollection.InsertOneAsync(mensaje);

                _logger.LogInformation("Mensaje automático creado con ID: {Id}", mensaje.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear mensaje automático");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un mensaje automático existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, MensajeAutomatico mensaje)
        {
            try
            {
                _logger.LogInformation("Actualizando mensaje automático con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                mensaje.Id = id;
                var filter = Builders<MensajeAutomatico>.Filter.Eq(m => m.Id, id);
                var result = await _mensajesCollection.ReplaceOneAsync(filter, mensaje);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Mensaje automático actualizado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró mensaje automático con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar mensaje automático con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina un mensaje automático
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando mensaje automático con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<MensajeAutomatico>.Filter.Eq(m => m.Id, id);
                var result = await _mensajesCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Mensaje automático eliminado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró mensaje automático con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar mensaje automático con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Busca mensajes automáticos por tipo
        /// </summary>
        public async Task<List<MensajeAutomatico>> GetByTipoAsync(string tipo)
        {
            try
            {
                _logger.LogInformation("Buscando mensajes automáticos por tipo: {Tipo}", tipo);

                var filter = Builders<MensajeAutomatico>.Filter.Eq(m => m.Tipo, tipo);
                return await _mensajesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar mensajes por tipo: {Tipo}", tipo);
                throw;
            }
        }

        /// <summary>
        /// Obtiene mensajes automáticos activos
        /// </summary>
        public async Task<List<MensajeAutomatico>> GetActivosAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo mensajes automáticos activos");

                var filter = Builders<MensajeAutomatico>.Filter.Eq(m => m.Activo, true);
                return await _mensajesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes activos");
                throw;
            }
        }
    }
}
