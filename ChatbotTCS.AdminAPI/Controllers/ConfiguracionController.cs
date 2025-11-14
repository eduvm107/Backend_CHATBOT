using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar configuraciones del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly ConfiguracionService _configuracionService;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(ConfiguracionService configuracionService, ILogger<ConfiguracionController> logger)
        {
            _configuracionService = configuracionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las configuraciones
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Configuracion>>> GetAll()
        {
            try
            {
                var configuraciones = await _configuracionService.GetAllAsync();
                return Ok(configuraciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones");
                return StatusCode(500, new { message = "Error al obtener configuraciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una configuración por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Configuracion>> GetById(string id)
        {
            try
            {
                var configuracion = await _configuracionService.GetByIdAsync(id);

                if (configuracion == null)
                {
                    _logger.LogWarning("Configuración no encontrada con ID: {Id}", id);
                    return NotFound(new { message = $"Configuración con ID {id} no encontrada" });
                }

                return Ok(configuracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener configuración", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva configuración
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Configuracion>> Create([FromBody] Configuracion configuracion)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear configuración");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(configuracion.Nombre))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                if (string.IsNullOrWhiteSpace(configuracion.Tipo))
                {
                    return BadRequest(new { message = "El tipo es requerido" });
                }

                await _configuracionService.CreateAsync(configuracion);

                _logger.LogInformation("Configuración creada con ID: {Id}", configuracion.Id);

                return CreatedAtAction(nameof(GetById), new { id = configuracion.Id }, configuracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear configuración");
                return StatusCode(500, new { message = "Error al crear configuración", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una configuración existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Configuracion configuracion)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar configuración");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(configuracion.Nombre))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                if (string.IsNullOrWhiteSpace(configuracion.Tipo))
                {
                    return BadRequest(new { message = "El tipo es requerido" });
                }

                var updated = await _configuracionService.UpdateAsync(id, configuracion);

                if (!updated)
                {
                    _logger.LogWarning("Configuración no encontrada para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Configuración con ID {id} no encontrada" });
                }

                _logger.LogInformation("Configuración actualizada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar configuración", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una configuración
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _configuracionService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Configuración no encontrada para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Configuración con ID {id} no encontrada" });
                }

                _logger.LogInformation("Configuración eliminada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar configuración con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar configuración", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene configuraciones por tipo
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Configuracion>>> GetByTipo(string tipo)
        {
            try
            {
                var configuraciones = await _configuracionService.GetByTipoAsync(tipo);
                return Ok(configuraciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones por tipo: {Tipo}", tipo);
                return StatusCode(500, new { message = "Error al obtener configuraciones por tipo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene configuraciones activas
        /// </summary>
        [HttpGet("activas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Configuracion>>> GetActivas()
        {
            try
            {
                var configuraciones = await _configuracionService.GetActivasAsync();
                return Ok(configuraciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones activas");
                return StatusCode(500, new { message = "Error al obtener configuraciones activas", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca una configuración por nombre
        /// </summary>
        [HttpGet("nombre/{nombre}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Configuracion>> GetByNombre(string nombre)
        {
            try
            {
                var configuracion = await _configuracionService.GetByNombreAsync(nombre);

                if (configuracion == null)
                {
                    _logger.LogWarning("Configuración no encontrada con nombre: {Nombre}", nombre);
                    return NotFound(new { message = $"Configuración con nombre {nombre} no encontrada" });
                }

                return Ok(configuracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar configuración por nombre: {Nombre}", nombre);
                return StatusCode(500, new { message = "Error al buscar configuración por nombre", error = ex.Message });
            }
        }
    }
}
