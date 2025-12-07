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
        private readonly DocumentoService _documentoService;
        private readonly ActividadService _actividadService;
        private readonly ConversacionService _conversacionService;

        /// <summary>
        /// Constructor del servicio con inyección de dependencias
        /// </summary>
        public UsuarioService(
            IOptions<MongoDBSettings> settings,
            ILogger<UsuarioService> logger,
            DocumentoService documentoService,
            ActividadService actividadService,
            ConversacionService conversacionService)
        {
            _logger = logger;
            _documentoService = documentoService;
            _actividadService = actividadService;
            _conversacionService = conversacionService;

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

        /// <summary>
        /// Alterna el estado de favorito de un recurso (documento, actividad, chat) para un usuario.
        /// Retorna true si se marcó como favorito, false si se desmarcó.
        /// </summary>
        public async Task<bool> ToggleFavoriteAsync(string usuarioId, string tipoRecurso, string recursoId)
        {
            try
            {
                _logger.LogInformation("Alternando favorito para usuario {UsuarioId}, tipo {TipoRecurso}, recurso {RecursoId}",
                    usuarioId, tipoRecurso, recursoId);

                // Verificar que el usuario existe
                var usuario = await GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    throw new ArgumentException($"Usuario con ID {usuarioId} no encontrado");
                }

                var tipoNormalizado = tipoRecurso.ToLowerInvariant();

                // 1. Para documentos y actividades: modificar arrays en Usuario
                if (tipoNormalizado == "documento" || tipoNormalizado == "actividad")
                {
                    string arrayToModify = tipoNormalizado == "documento" 
                        ? "favoritosDocumentos" 
                        : "favoritosActividades";

                    var filter = Builders<Usuario>.Filter.Eq(u => u.Id, usuarioId);

                    // Intentar QUITAR el recurso (Desmarcar) usando $pull
                    var pullUpdate = Builders<Usuario>.Update.Pull(arrayToModify, recursoId);
                    var pullResult = await _usuariosCollection.UpdateOneAsync(filter, pullUpdate);

                    if (pullResult.ModifiedCount > 0)
                    {
                        // Se removió el favorito
                        _logger.LogInformation("Recurso {RecursoId} desmarcado como favorito", recursoId);
                        return false;
                    }
                    else
                    {
                        // Si no se removió, entonces AGREGAR (Marcar) usando $addToSet
                        var pushUpdate = Builders<Usuario>.Update.AddToSet(arrayToModify, recursoId);
                        var pushResult = await _usuariosCollection.UpdateOneAsync(filter, pushUpdate);

                        if (pushResult.ModifiedCount > 0)
                        {
                            _logger.LogInformation("Recurso {RecursoId} marcado como favorito", recursoId);
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("No se pudo modificar favoritos para usuario {UsuarioId}", usuarioId);
                            return false;
                        }
                    }
                }
                // 2. Para chats/conversaciones: modificar el campo booleano "favorito" en la colección de conversaciones
                else if (tipoNormalizado == "chat" || tipoNormalizado == "conversacion")
                {
                    // Obtener la conversación actual
                    var conversacion = await _conversacionService.GetByIdAsync(recursoId);
                    if (conversacion == null)
                    {
                        throw new ArgumentException($"Conversación con ID {recursoId} no encontrada");
                    }

                    // Verificar que la conversación pertenece al usuario
                    if (conversacion.UsuarioId != usuarioId)
                    {
                        throw new ArgumentException($"La conversación {recursoId} no pertenece al usuario {usuarioId}");
                    }

                    // Alternar el estado de favorito
                    bool nuevoEstado = !conversacion.Favorito;
                    await _conversacionService.UpdateFavoritoAsync(recursoId, nuevoEstado);

                    _logger.LogInformation("Conversación {RecursoId} {Estado} como favorita", 
                        recursoId, nuevoEstado ? "marcada" : "desmarcada");

                    return nuevoEstado;
                }
                else
                {
                    throw new ArgumentException($"Tipo de recurso '{tipoRecurso}' no soportado para favoritos. Valores válidos: documento, actividad, chat");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al alternar favorito para usuario {UsuarioId}", usuarioId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todos los recursos favoritos de un usuario (documentos, actividades y chats).
        /// Retorna una lista unificada de RecursoFavorito.
        /// </summary>
        public async Task<List<RecursoFavorito>> GetMisFavoritosAsync(string usuarioId)
        {
            try
            {
                _logger.LogInformation("Obteniendo favoritos para usuario {UsuarioId}", usuarioId);

                var usuario = await GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UsuarioId}", usuarioId);
                    return new List<RecursoFavorito>();
                }

                var listaFavoritosFinal = new List<RecursoFavorito>();

                // 1. Extraer IDs de documentos y actividades favoritos
                var docIds = usuario.favoritosDocumentos ?? new List<string>();
                var actIds = usuario.favoritosActividades ?? new List<string>();

                // 2. Consultar y mapear Documentos Favoritos
                if (docIds.Any())
                {
                    var documentos = await _documentoService.FindByIdsAsync(docIds);
                    listaFavoritosFinal.AddRange(documentos.Select(doc => new RecursoFavorito
                    {
                        Id = doc.Id,
                        Tipo = "documento",
                        Titulo = doc.Titulo,
                        Descripcion = doc.Descripcion,
                        Url = doc.Url,
                        FechaRelevante = doc.FechaPublicacion
                    }));
                    _logger.LogInformation("Se encontraron {Count} documentos favoritos", documentos.Count);
                }

                // 3. Consultar y mapear Actividades Favoritas
                if (actIds.Any())
                {
                    var actividades = await _actividadService.FindByIdsAsync(actIds);
                    listaFavoritosFinal.AddRange(actividades.Select(act => new RecursoFavorito
                    {
                        Id = act.Id,
                        Tipo = "actividad",
                        Titulo = act.Titulo,
                        Descripcion = act.Descripcion,
                        Url = null,
                        FechaRelevante = act.FechaDeActividad
                    }));
                    _logger.LogInformation("Se encontraron {Count} actividades favoritas", actividades.Count);
                }

                // 4. Consultar conversaciones favoritas (basadas en el campo Favorito booleano)
                var chatsFavoritos = await _conversacionService.GetFavoritosByUsuarioAsync(usuarioId);
                if (chatsFavoritos.Any())
                {
                    listaFavoritosFinal.AddRange(chatsFavoritos.Select(chat => new RecursoFavorito
                    {
                        Id = chat.Id,
                        Tipo = "chat",
                        Titulo = $"Conversación - {chat.FechaInicio:dd MMM yyyy}",
                        Descripcion = chat.Mensajes.FirstOrDefault()?.Contenido ?? "Conversación sin mensajes",
                        Url = null,
                        FechaRelevante = chat.FechaUltimaMensaje
                    }));
                    _logger.LogInformation("Se encontraron {Count} conversaciones favoritas", chatsFavoritos.Count);
                }

                _logger.LogInformation("Total de favoritos encontrados: {Count} (Documentos: {DocCount}, Actividades: {ActCount}, Chats: {ChatCount})",
                    listaFavoritosFinal.Count, docIds.Count, actIds.Count, chatsFavoritos.Count);

                return listaFavoritosFinal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener favoritos para usuario {UsuarioId}", usuarioId);
                return new List<RecursoFavorito>();
            }
        }

        /// Busca un usuario por token de restablecimiento de contraseña
        /// </summary>
        public async Task<Usuario?> GetByResetTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Buscando usuario por token de restablecimiento");

                var filter = Builders<Usuario>.Filter.Eq(u => u.ResetPasswordToken, token);
                return await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por token de restablecimiento");

                throw;
            }
        }

        // ==========================================
        //  MÉTODOS EXCLUSIVOS PARA EL CHATBOT (RAG)
        // ==========================================

        /// <summary>
        /// Método exclusivo para que el Chatbot busque usuarios sin afectar la lógica principal.
        /// </summary>
        public async Task<Usuario?> GetUsuarioParaChatAsync(string id)
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
        /// Genera una contraseña temporal y la envía al correo del usuario
        /// </summary>
        public async Task<bool> GenerarYEnviarContrasenaTemporalAsync(string email, string smtpUser, string smtpPassword)
        {
            try
            {
                _logger.LogInformation("Generando contraseña temporal para usuario con email: {Email}", email);

                var usuario = await GetByEmailAsync(email);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para email: {Email}", email);
                    return false;
                }

                // Generar contraseña temporal
                var contrasenaTemporal = Guid.NewGuid().ToString().Substring(0, 8);
                usuario.Contraseña = contrasenaTemporal; // Aquí puedes encriptar la contraseña si es necesario
                usuario.FechaActualizacion = DateTime.UtcNow;

                await UpdateAsync(usuario.Id!, usuario);

                // Enviar correo electrónico
                using (var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(smtpUser),
                        Subject = "Contraseña Temporal",
                        Body = $"Hola {usuario.Nombre},\n\nTu nueva contraseña temporal es: {contrasenaTemporal}\n\nPor favor, cámbiala después de iniciar sesión.",
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                }

                _logger.LogInformation("Contraseña temporal enviada exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar o enviar contraseña temporal para email: {Email}", email);
                return false;
            }
        }
    }

}
