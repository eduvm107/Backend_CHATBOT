using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para gestionar usuarios
    /// </summary>
    public class UsuarioService
    {
        private readonly IMongoCollection<Usuario> _usuariosCollection;
        private readonly ILogger<UsuarioService> _logger;

        /// <summary>
        /// Constructor del servicio
        /// </summary>
        public UsuarioService(IOptions<MongoDBSettings> settings, ILogger<UsuarioService> logger)
        {
            _logger = logger;

            try
            {
                var mongoClient = new MongoClient(settings.Value.ConnectionString);
                var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
                _usuariosCollection = database.GetCollection<Usuario>("usuarios");

                _logger.LogInformation("Conexión a colección usuarios establecida");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con MongoDB - usuarios");
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<Usuario>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los usuarios");
                return await _usuariosCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<Usuario?> GetByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuario con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return null;
                }

                var filter = Builders<Usuario>.Filter.Eq(u => u.Id, id);
                return await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public async Task CreateAsync(Usuario usuario)
        {
            try
            {
                _logger.LogInformation("Creando nuevo usuario: {Email}", usuario.Email);

                usuario.FechaCreacion = DateTime.UtcNow;
                usuario.FechaActualizacion = DateTime.UtcNow;
                await _usuariosCollection.InsertOneAsync(usuario);

                _logger.LogInformation("Usuario creado con ID: {Id}", usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        public async Task<bool> UpdateAsync(string id, Usuario usuario)
        {
            try
            {
                _logger.LogInformation("Actualizando usuario con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                usuario.FechaActualizacion = DateTime.UtcNow;
                usuario.Id = id;
                var filter = Builders<Usuario>.Filter.Eq(u => u.Id, id);
                var result = await _usuariosCollection.ReplaceOneAsync(filter, usuario);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Usuario actualizado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró usuario con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                _logger.LogInformation("Eliminando usuario con ID: {Id}", id);

                if (!ObjectId.TryParse(id, out _))
                {
                    _logger.LogWarning("ID inválido: {Id}", id);
                    return false;
                }

                var filter = Builders<Usuario>.Filter.Eq(u => u.Id, id);
                var result = await _usuariosCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Usuario eliminado exitosamente");
                    return true;
                }
                else
                {
                    _logger.LogWarning("No se encontró usuario con ID: {Id}", id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Busca un usuario por email
        /// </summary>
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Buscando usuario por email: {Email}", email);

                var filter = Builders<Usuario>.Filter.Eq(u => u.Email, email);
                return await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Busca un usuario por DNI
        /// </summary>
        public async Task<Usuario?> GetByDniAsync(string dni)
        {
            try
            {
                _logger.LogInformation("Buscando usuario por DNI: {Dni}", dni);

                var filter = Builders<Usuario>.Filter.Eq(u => u.Dni, dni);
                return await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por DNI: {Dni}", dni);
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuarios por estado de onboarding
        /// </summary>
        public async Task<List<Usuario>> GetByEstadoOnboardingAsync(string estado)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios por estado de onboarding: {Estado}", estado);

                var filter = Builders<Usuario>.Filter.Eq(u => u.EstadoOnboarding, estado);
                return await _usuariosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por estado de onboarding: {Estado}", estado);
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuarios activos
        /// </summary>
        public async Task<List<Usuario>> GetActivosAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios activos");

                var filter = Builders<Usuario>.Filter.Eq(u => u.Activo, true);
                return await _usuariosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos");
                throw;
            }
        }

        /// <summary>
        /// Obtiene usuarios por departamento
        /// </summary>
        public async Task<List<Usuario>> GetByDepartamentoAsync(string departamento)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios por departamento: {Departamento}", departamento);

                var filter = Builders<Usuario>.Filter.Eq(u => u.Departamento, departamento);
                return await _usuariosCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por departamento: {Departamento}", departamento);
                throw;
            }
        }
    }
}
