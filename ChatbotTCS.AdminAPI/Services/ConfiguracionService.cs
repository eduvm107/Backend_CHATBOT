using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar configuraciones del sistema
    /// </summary>
    public class ConfiguracionService
    {
        private readonly IMongoCollection<Configuracion> _configuracionCollection;
        private readonly ILogger<ConfiguracionService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public ConfiguracionService(IOptions<MongoDBSettings> settings, ILogger<ConfiguracionService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _configuracionCollection = database.GetCollection<Configuracion>("configuracion");

                _logger.LogInformation("Conexión a colección configuracion establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - configuracion");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las configuraciones
        /// </summary>
        public async Task<List<Configuracion>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las configuraciones");
                return await _configuracionCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una configuración por ID
        /// </summary>
        public async Task<Configuracion?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo configuración con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Id, id);
                return await _configuracionCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva configuración
        /// </summary>
        public async Task CreateAsync(Configuracion configuracion)
        {
            try
            {
                _logger.LogInformation("Creando nueva configuración: {Nombre}", configuracion.Nombre);

                configuracion.FechaCreacion = DateTime.UtcNow;
                configuracion.FechaActualizacion = DateTime.UtcNow;
                await _configuracionCollection.InsertOneAsync(configuracion);

                _logger.LogInformation("Configuración creada con ID: {Id}", configuracion.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear configuración");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una configuración existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, Configuracion configuracion)
        {
            try
            {
                _logger.LogInformation("Actualizando configuración con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                configuracion.FechaActualizacion = DateTime.UtcNow;
                configuracion.Id = id;
                var filter = Builders<Configuracion>.Filter.Eq(c => c.Id, id);
                var result = await _configuracionCollection.ReplaceOneAsync(filter, configuracion);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Configuración actualizada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró configuración con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una configuración
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando configuración con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Id, id);
                var result = await _configuracionCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Configuración eliminada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró configuración con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar configuración con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene configuraciones por tipo
        /// </summary>
        public async Task<List<Configuracion>> GetByTipoAsync(string tipo)
        {
            try
            {
                _logger.LogInformation("Obteniendo configuraciones por tipo: {Tipo}", tipo);

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, tipo);
                return await _configuracionCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones por tipo: {Tipo}", tipo);
                throw;
            }
        }

        /// <summary>
        /// Obtiene configuraciones activas
        /// </summary>
        public async Task<List<Configuracion>> GetActivasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo configuraciones activas");

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Activo, true);
                return await _configuracionCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones activas");
                throw;
            }
        }

        /// <summary>
        /// Busca una configuración por nombre
        /// </summary>
        public async Task<Configuracion?> GetByNombreAsync(string nombre)
        {
            try
            {
                _logger.LogInformation("Buscando configuración por nombre: {Nombre}", nombre);

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Nombre, nombre);
                return await _configuracionCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar configuración por nombre: {Nombre}", nombre);
                throw;
            }
        }
    }
}
