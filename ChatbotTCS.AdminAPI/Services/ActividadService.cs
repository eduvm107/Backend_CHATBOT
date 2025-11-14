using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar actividades de onboarding
    /// </summary>
    public class ActividadService
    {
        private readonly IMongoCollection<Actividad> _actividadesCollection;
        private readonly ILogger<ActividadService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public ActividadService(IOptions<MongoDBSettings> settings, ILogger<ActividadService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _actividadesCollection = database.GetCollection<Actividad>("actividades");

                _logger.LogInformation("Conexión a colección actividades establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - actividades");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las actividades
        /// </summary>
        public async Task<List<Actividad>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las actividades");
                return await _actividadesCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una actividad por ID
        /// </summary>
        public async Task<Actividad?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo actividad con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<Actividad>.Filter.Eq(a => a.Id, id);
                return await _actividadesCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva actividad
        /// </summary>
        public async Task CreateAsync(Actividad actividad)
        {
            try
            {
                _logger.LogInformation("Creando nueva actividad: {Titulo}", actividad.Titulo);

                actividad.FechaCreacion = DateTime.UtcNow;
                await _actividadesCollection.InsertOneAsync(actividad);

                _logger.LogInformation("Actividad creada con ID: {Id}", actividad.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear actividad");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una actividad existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, Actividad actividad)
        {
            try
            {
                _logger.LogInformation("Actualizando actividad con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                actividad.Id = id;
                var filter = Builders<Actividad>.Filter.Eq(a => a.Id, id);
                var result = await _actividadesCollection.ReplaceOneAsync(filter, actividad);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Actividad actualizada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró actividad con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar actividad con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina una actividad
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando actividad con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Actividad>.Filter.Eq(a => a.Id, id);
                var result = await _actividadesCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Actividad eliminada exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró actividad con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar actividad con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene actividades por día
        /// </summary>
        public async Task<List<Actividad>> GetByDiaAsync(int dia)
        {
            try
            {
                _logger.LogInformation("Obteniendo actividades del día: {Dia}", dia);

                var filter = Builders<Actividad>.Filter.Eq(a => a.Dia, dia);
                return await _actividadesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades del día: {Dia}", dia);
                throw;
            }
        }

        /// <summary>
        /// Obtiene actividades por tipo
        /// </summary>
        public async Task<List<Actividad>> GetByTipoAsync(string tipo)
        {
            try
            {
                _logger.LogInformation("Obteniendo actividades por tipo: {Tipo}", tipo);

                var filter = Builders<Actividad>.Filter.Eq(a => a.Tipo, tipo);
                return await _actividadesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades por tipo: {Tipo}", tipo);
                throw;
            }
        }

        /// <summary>
        /// Obtiene actividades obligatorias
        /// </summary>
        public async Task<List<Actividad>> GetObligatoriasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo actividades obligatorias");

                var filter = Builders<Actividad>.Filter.Eq(a => a.Obligatorio, true);
                return await _actividadesCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades obligatorias");
                throw;
            }
        }
    }
}
