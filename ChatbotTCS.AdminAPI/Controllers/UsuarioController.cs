using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(UsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Usuario>>> GetAll()
        {
            try
            {
                var usuarios = await _usuarioService.GetAllAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { message = "Error al obtener usuarios", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> GetById(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetByIdAsync(id);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con ID: {Id}", id);
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> Create([FromBody] Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear usuario");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(usuario.Email))
                {
                    return BadRequest(new { message = "El email es requerido" });
                }

                if (string.IsNullOrWhiteSpace(usuario.Nombre))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                await _usuarioService.CreateAsync(usuario);

                _logger.LogInformation("Usuario creado con ID: {Id}", usuario.Id);

                return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Usuario usuario)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar usuario");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(usuario.Email))
                {
                    return BadRequest(new { message = "El email es requerido" });
                }

                if (string.IsNullOrWhiteSpace(usuario.Nombre))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                var updated = await _usuarioService.UpdateAsync(id, usuario);

                if (!updated)
                {
                    _logger.LogWarning("Usuario no encontrado para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
                }

                _logger.LogInformation("Usuario actualizado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un usuario
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _usuarioService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Usuario no encontrado para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
                }

                _logger.LogInformation("Usuario eliminado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca un usuario por email
        /// </summary>
        [HttpGet("email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> GetByEmail(string email)
        {
            try
            {
                var usuario = await _usuarioService.GetByEmailAsync(email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con email: {Email}", email);
                    return NotFound(new { message = $"Usuario con email {email} no encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email);
                return StatusCode(500, new { message = "Error al buscar usuario por email", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca un usuario por DNI
        /// </summary>
        [HttpGet("dni/{dni}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> GetByDni(string dni)
        {
            try
            {
                var usuario = await _usuarioService.GetByDniAsync(dni);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado con DNI: {Dni}", dni);
                    return NotFound(new { message = $"Usuario con DNI {dni} no encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por DNI: {Dni}", dni);
                return StatusCode(500, new { message = "Error al buscar usuario por DNI", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene usuarios por estado de onboarding
        /// </summary>
        [HttpGet("onboarding/{estado}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Usuario>>> GetByEstadoOnboarding(string estado)
        {
            try
            {
                var usuarios = await _usuarioService.GetByEstadoOnboardingAsync(estado);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por estado de onboarding: {Estado}", estado);
                return StatusCode(500, new { message = "Error al obtener usuarios por estado de onboarding", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene usuarios activos
        /// </summary>
        [HttpGet("activos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Usuario>>> GetActivos()
        {
            try
            {
                var usuarios = await _usuarioService.GetActivosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos");
                return StatusCode(500, new { message = "Error al obtener usuarios activos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene usuarios por departamento
        /// </summary>
        [HttpGet("departamento/{departamento}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Usuario>>> GetByDepartamento(string departamento)
        {
            try
            {
                var usuarios = await _usuarioService.GetByDepartamentoAsync(departamento);
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por departamento: {Departamento}", departamento);
                return StatusCode(500, new { message = "Error al obtener usuarios por departamento", error = ex.Message });
            }
        }

        /// <summary>
        /// Alterna el estado de favorito de un recurso para un usuario
        /// </summary>
        [HttpPost("{id}/favoritos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> ToggleFavorite(string id, [FromBody] FavoritoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.TipoRecurso))
                {
                    return BadRequest(new { message = "El tipo de recurso es requerido" });
                }

                if (string.IsNullOrWhiteSpace(request.RecursoId))
                {
                    return BadRequest(new { message = "El ID del recurso es requerido" });
                }

                var isFavorito = await _usuarioService.ToggleFavoriteAsync(id, request.TipoRecurso, request.RecursoId);

                _logger.LogInformation("Estado de favorito alternado para usuario {Id}, recurso {RecursoId}", id, request.RecursoId);
                
                return Ok(new { 
                    message = isFavorito ? "Recurso marcado como favorito" : "Recurso desmarcado como favorito",
                    isFavorito = isFavorito
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Tipo de recurso no válido");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al alternar favorito para usuario {Id}", id);
                return StatusCode(500, new { message = "Error al alternar favorito", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todos los recursos favoritos de un usuario (documentos, actividades, chats)
        /// </summary>
        [HttpGet("{id}/favoritos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RecursoFavorito>>> GetMisFavoritos(string id)
        {
            try
            {
                var favoritos = await _usuarioService.GetMisFavoritosAsync(id);
                
                if (favoritos == null || !favoritos.Any())
                {
                    _logger.LogInformation("No se encontraron favoritos para usuario {Id}", id);
                    return Ok(new List<RecursoFavorito>());
                }

                return Ok(favoritos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener favoritos para usuario {Id}", id);
                return StatusCode(500, new { message = "Error al obtener favoritos", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo para la solicitud de alternar favorito
    /// </summary>
    public class FavoritoRequest
    {
        /// <summary>
        /// Tipo de recurso: "documento", "actividad", "chat"
        /// </summary>
        public string TipoRecurso { get; set; } = string.Empty;

        /// <summary>
        /// ID del recurso a marcar/desmarcar como favorito
        /// </summary>
        public string RecursoId { get; set; } = string.Empty;
    }
}
