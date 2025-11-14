using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar conversaciones del chatbot
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConversacionController : ControllerBase
    {
        private readonly ConversacionService _conversacionService;
        private readonly ILogger<ConversacionController> _logger;

        public ConversacionController(ConversacionService conversacionService, ILogger<ConversacionController> logger)
        {
            _conversacionService = conversacionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las conversaciones
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Conversacion>>> GetAll()
        {
            try
            {
                var conversaciones = await _conversacionService.GetAllAsync();
                return Ok(conversaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones");
                return StatusCode(500, new { message = "Error al obtener conversaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una conversación por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Conversacion>> GetById(string id)
        {
            try
            {
                var conversacion = await _conversacionService.GetByIdAsync(id);

                if (conversacion == null)
                {
                    _logger.LogWarning("Conversación no encontrada con ID: {Id}", id);
                    return NotFound(new { message = $"Conversación con ID {id} no encontrada" });
                }

                return Ok(conversacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversación con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener conversación", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva conversación
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Conversacion>> Create([FromBody] Conversacion conversacion)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear conversación");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(conversacion.UsuarioId))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
                }

                await _conversacionService.CreateAsync(conversacion);

                _logger.LogInformation("Conversación creada con ID: {Id}", conversacion.Id);

                return CreatedAtAction(nameof(GetById), new { id = conversacion.Id }, conversacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conversación");
                return StatusCode(500, new { message = "Error al crear conversación", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una conversación existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Conversacion conversacion)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar conversación");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(conversacion.UsuarioId))
                {
                    return BadRequest(new { message = "El ID de usuario es requerido" });
                }

                var updated = await _conversacionService.UpdateAsync(id, conversacion);

                if (!updated)
                {
                    _logger.LogWarning("Conversación no encontrada para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Conversación con ID {id} no encontrada" });
                }

                _logger.LogInformation("Conversación actualizada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar conversación con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar conversación", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una conversación
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _conversacionService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Conversación no encontrada para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Conversación con ID {id} no encontrada" });
                }

                _logger.LogInformation("Conversación eliminada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar conversación con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar conversación", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene conversaciones por usuario
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Conversacion>>> GetByUsuarioId(string usuarioId)
        {
            try
            {
                var conversaciones = await _conversacionService.GetByUsuarioIdAsync(usuarioId);
                return Ok(conversaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones del usuario: {UsuarioId}", usuarioId);
                return StatusCode(500, new { message = "Error al obtener conversaciones del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene conversaciones activas
        /// </summary>
        [HttpGet("activas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Conversacion>>> GetActivas()
        {
            try
            {
                var conversaciones = await _conversacionService.GetActivasAsync();
                return Ok(conversaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones activas");
                return StatusCode(500, new { message = "Error al obtener conversaciones activas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene conversaciones resueltas
        /// </summary>
        [HttpGet("resueltas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Conversacion>>> GetResueltas()
        {
            try
            {
                var conversaciones = await _conversacionService.GetResueltasAsync();
                return Ok(conversaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conversaciones resueltas");
                return StatusCode(500, new { message = "Error al obtener conversaciones resueltas", error = ex.Message });
            }
        }

        /// <summary>
        /// Agrega un mensaje a una conversación existente
        /// </summary>
        [HttpPost("{id}/mensajes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddMensaje(string id, [FromBody] Mensaje mensaje)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al agregar mensaje");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(mensaje.Contenido))
                {
                    return BadRequest(new { message = "El contenido del mensaje es requerido" });
                }

                var added = await _conversacionService.AddMensajeAsync(id, mensaje);

                if (!added)
                {
                    _logger.LogWarning("Conversación no encontrada para agregar mensaje con ID: {Id}", id);
                    return NotFound(new { message = $"Conversación con ID {id} no encontrada" });
                }

                _logger.LogInformation("Mensaje agregado a conversación con ID: {Id}", id);
                return Ok(new { message = "Mensaje agregado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar mensaje a conversación con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al agregar mensaje", error = ex.Message });
            }
        }
    }
}
