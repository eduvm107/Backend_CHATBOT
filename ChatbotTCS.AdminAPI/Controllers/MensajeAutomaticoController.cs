using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar mensajes automケticos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MensajeAutomaticoController : ControllerBase
    {
        private readonly MensajeAutomaticoService _mensajeService;
        private readonly ILogger<MensajeAutomaticoController> _logger;

        public MensajeAutomaticoController(MensajeAutomaticoService mensajeService, ILogger<MensajeAutomaticoController> logger)
        {
            _mensajeService = mensajeService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los mensajes automケticos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MensajeAutomatico>>> GetAll()
        {
            try
            {
                var mensajes = await _mensajeService.GetAllAsync();
                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes automケticos");
                return StatusCode(500, new { message = "Error al obtener mensajes automケticos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un mensaje automケtico por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MensajeAutomatico>> GetById(string id)
        {
            try
            {
                var mensaje = await _mensajeService.GetByIdAsync(id);

                if (mensaje == null)
                {
                    _logger.LogWarning("Mensaje automケtico no encontrado con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automケtico con ID {id} no encontrado" });
                }

                return Ok(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensaje automケtico con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener mensaje automケtico", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo mensaje automケtico
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MensajeAutomatico>> Create([FromBody] MensajeAutomatico mensaje)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo invケlido al crear mensaje automケtico");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(mensaje.Titulo))
                {
                    return BadRequest(new { message = "El tヴtulo es requerido" });
                }

                if (string.IsNullOrWhiteSpace(mensaje.Contenido))
                {
                    return BadRequest(new { message = "El contenido es requerido" });
                }

                await _mensajeService.CreateAsync(mensaje);

                _logger.LogInformation("Mensaje automケtico creado con ID: {Id}", mensaje.Id);

                return CreatedAtAction(nameof(GetById), new { id = mensaje.Id }, mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear mensaje automケtico");
                return StatusCode(500, new { message = "Error al crear mensaje automケtico", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un mensaje automケtico existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] MensajeAutomatico mensaje)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo invケlido al actualizar mensaje automケtico");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(mensaje.Titulo))
                {
                    return BadRequest(new { message = "El tヴtulo es requerido" });
                }

                if (string.IsNullOrWhiteSpace(mensaje.Contenido))
                {
                    return BadRequest(new { message = "El contenido es requerido" });
                }

                var updated = await _mensajeService.UpdateAsync(id, mensaje);

                if (!updated)
                {
                    _logger.LogWarning("Mensaje automケtico no encontrado para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automケtico con ID {id} no encontrado" });
                }

                _logger.LogInformation("Mensaje automケtico actualizado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar mensaje automケtico con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar mensaje automケtico", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un mensaje automケtico
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _mensajeService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Mensaje automケtico no encontrado para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automケtico con ID {id} no encontrado" });
                }

                _logger.LogInformation("Mensaje automケtico eliminado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar mensaje automケtico con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar mensaje automケtico", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene mensajes automケticos por tipo
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MensajeAutomatico>>> GetByTipo(string tipo)
        {
            try
            {
                var mensajes = await _mensajeService.GetByTipoAsync(tipo);
                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes por tipo: {Tipo}", tipo);
                return StatusCode(500, new { message = "Error al obtener mensajes por tipo", error = ex.Message });
            }
        }

        /// <summary>
        /// Activa o desactiva un mensaje automケtico (solo el campo Activo)
        /// </summary>
        [HttpPatch("{id}/activo")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateActivo(string id, [FromBody] bool activo)
        {
            try
            {
                var mensaje = await _mensajeService.GetByIdAsync(id);

                if (mensaje == null)
                {
                    _logger.LogWarning("Mensaje automケtico no encontrado para actualizar Activo con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automケtico con ID {id} no encontrado" });
                }

                mensaje.Activo = activo;

                var updated = await _mensajeService.UpdateAsync(id, mensaje);

                if (!updated)
                {
                    _logger.LogWarning("No se pudo actualizar el estado Activo para mensaje automケtico con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automケtico con ID {id} no encontrado" });
                }

                _logger.LogInformation("Estado Activo actualizado para mensaje automケtico con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar Activo de mensaje automケtico con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar Activo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene mensajes automケticos activos
        /// </summary>
        [HttpGet("activos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<MensajeAutomatico>>> GetActivos()
        {
            try
            {
                var mensajes = await _mensajeService.GetActivosAsync();
                return Ok(mensajes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensajes activos");
                return StatusCode(500, new { message = "Error al obtener mensajes activos", error = ex.Message });
            }
        }
    }
}
