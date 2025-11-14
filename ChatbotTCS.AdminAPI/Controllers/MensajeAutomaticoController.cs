using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar mensajes automáticos
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
        /// Obtiene todos los mensajes automáticos
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
                _logger.LogError(ex, "Error al obtener mensajes automáticos");
                return StatusCode(500, new { message = "Error al obtener mensajes automáticos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un mensaje automático por ID
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
                    _logger.LogWarning("Mensaje automático no encontrado con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automático con ID {id} no encontrado" });
                }

                return Ok(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mensaje automático con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener mensaje automático", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo mensaje automático
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
                    _logger.LogWarning("Modelo inválido al crear mensaje automático");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(mensaje.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(mensaje.Contenido))
                {
                    return BadRequest(new { message = "El contenido es requerido" });
                }

                await _mensajeService.CreateAsync(mensaje);

                _logger.LogInformation("Mensaje automático creado con ID: {Id}", mensaje.Id);

                return CreatedAtAction(nameof(GetById), new { id = mensaje.Id }, mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear mensaje automático");
                return StatusCode(500, new { message = "Error al crear mensaje automático", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un mensaje automático existente
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
                    _logger.LogWarning("Modelo inválido al actualizar mensaje automático");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(mensaje.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(mensaje.Contenido))
                {
                    return BadRequest(new { message = "El contenido es requerido" });
                }

                var updated = await _mensajeService.UpdateAsync(id, mensaje);

                if (!updated)
                {
                    _logger.LogWarning("Mensaje automático no encontrado para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automático con ID {id} no encontrado" });
                }

                _logger.LogInformation("Mensaje automático actualizado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar mensaje automático con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar mensaje automático", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un mensaje automático
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
                    _logger.LogWarning("Mensaje automático no encontrado para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Mensaje automático con ID {id} no encontrado" });
                }

                _logger.LogInformation("Mensaje automático eliminado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar mensaje automático con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar mensaje automático", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene mensajes automáticos por tipo
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
        /// Obtiene mensajes automáticos activos
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
