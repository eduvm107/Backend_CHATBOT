using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar documentos
    /// </summary>
    public class DocumentoService
    {
        private readonly IMongoCollection<Documento> _documentosCollection;
        private readonly ILogger<DocumentoService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public DocumentoService(IOptions<MongoDBSettings> settings, ILogger<DocumentoService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _documentosCollection = database.GetCollection<Documento>("documentos");

                _logger.LogInformation("Conexión a colección documentos establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los documentos
        /// </summary>
        public async Task<List<Documento>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los documentos");
                return await _documentosCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un documento por ID
        /// </summary>
        public async Task<Documento?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo documento con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<Documento>.Filter.Eq(d => d.Id, id);
                return await _documentosCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documento con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo documento
        /// </summary>
        public async Task CreateAsync(Documento documento)
        {
            try
            {
                _logger.LogInformation("Creando nuevo documento: {Titulo}", documento.Titulo);

                documento.FechaPublicacion = DateTime.UtcNow;
                documento.FechaActualizacion = DateTime.UtcNow;
                await _documentosCollection.InsertOneAsync(documento);

                _logger.LogInformation("Documento creado con ID: {Id}", documento.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear documento");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un documento existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, Documento documento)
        {
            try
            {
                _logger.LogInformation("Actualizando documento con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                documento.FechaActualizacion = DateTime.UtcNow;
                documento.Id = id;
                var filter = Builders<Documento>.Filter.Eq(d => d.Id, id);
                var result = await _documentosCollection.ReplaceOneAsync(filter, documento);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Documento actualizado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró documento con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar documento con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina un documento
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando documento con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Documento>.Filter.Eq(d => d.Id, id);
                var result = await _documentosCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Documento eliminado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró documento con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar documento con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Busca documentos por categoría
        /// </summary>
        public async Task<List<Documento>> GetByCategoriaAsync(string categoria)
        {
            try
            {
                _logger.LogInformation("Buscando documentos por categoría: {Categoria}", categoria);

                var filter = Builders<Documento>.Filter.Eq(d => d.Categoria, categoria);
                return await _documentosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar documentos por categoría: {Categoria}", categoria);
                throw;
            }
        }

        /// <summary>
        /// Busca documentos por tipo
        /// </summary>
        public async Task<List<Documento>> GetByTipoAsync(string tipo)
        {
            try
            {
                _logger.LogInformation("Buscando documentos por tipo: {Tipo}", tipo);

                var filter = Builders<Documento>.Filter.Eq(d => d.Tipo, tipo);
                return await _documentosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar documentos por tipo: {Tipo}", tipo);
                throw;
            }
        }

        /// <summary>
        /// Busca documentos por tags
        /// </summary>
        public async Task<List<Documento>> SearchByTagsAsync(string tag)
        {
            try
            {
                _logger.LogInformation("Buscando documentos por tag: {Tag}", tag);

                var filter = Builders<Documento>.Filter.AnyEq(d => d.Tags, tag);
                return await _documentosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar documentos por tag: {Tag}", tag);
                throw;
            }
        }
    }
}
