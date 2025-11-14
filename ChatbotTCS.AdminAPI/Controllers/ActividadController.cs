using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar actividades de onboarding
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ActividadController : ControllerBase
    {
        private readonly ActividadService _actividadService;
        private readonly ILogger<ActividadController> _logger;

        public ActividadController(ActividadService actividadService, ILogger<ActividadController> logger)
        {
            _actividadService = actividadService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las actividades
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Actividad>>> GetAll()
        {
            try
            {
                var actividades = await _actividadService.GetAllAsync();
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades");
                return StatusCode(500, new { message = "Error al obtener actividades", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una actividad por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Actividad>> GetById(string id)
        {
            try
            {
                var actividad = await _actividadService.GetByIdAsync(id);

                if (actividad == null)
                {
                    _logger.LogWarning("Actividad no encontrada con ID: {Id}", id);
                    return NotFound(new { message = $"Actividad con ID {id} no encontrada" });
                }

                return Ok(actividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener actividad", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva actividad
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Actividad>> Create([FromBody] Actividad actividad)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear actividad");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(actividad.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(actividad.Descripcion))
                {
                    return BadRequest(new { message = "La descripción es requerida" });
                }

                await _actividadService.CreateAsync(actividad);

                _logger.LogInformation("Actividad creada con ID: {Id}", actividad.Id);

                return CreatedAtAction(nameof(GetById), new { id = actividad.Id }, actividad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear actividad");
                return StatusCode(500, new { message = "Error al crear actividad", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una actividad existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Actividad actividad)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar actividad");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(actividad.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(actividad.Descripcion))
                {
                    return BadRequest(new { message = "La descripción es requerida" });
                }

                var updated = await _actividadService.UpdateAsync(id, actividad);

                if (!updated)
                {
                    _logger.LogWarning("Actividad no encontrada para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Actividad con ID {id} no encontrada" });
                }

                _logger.LogInformation("Actividad actualizada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar actividad con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar actividad", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una actividad
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _actividadService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Actividad no encontrada para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Actividad con ID {id} no encontrada" });
                }

                _logger.LogInformation("Actividad eliminada con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar actividad con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar actividad", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene actividades por día
        /// </summary>
        [HttpGet("dia/{dia}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Actividad>>> GetByDia(int dia)
        {
            try
            {
                var actividades = await _actividadService.GetByDiaAsync(dia);
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades del día: {Dia}", dia);
                return StatusCode(500, new { message = "Error al obtener actividades por día", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene actividades por tipo
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Actividad>>> GetByTipo(string tipo)
        {
            try
            {
                var actividades = await _actividadService.GetByTipoAsync(tipo);
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades por tipo: {Tipo}", tipo);
                return StatusCode(500, new { message = "Error al obtener actividades por tipo", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene actividades obligatorias
        /// </summary>
        [HttpGet("obligatorias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Actividad>>> GetObligatorias()
        {
            try
            {
                var actividades = await _actividadService.GetObligatoriasAsync();
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividades obligatorias");
                return StatusCode(500, new { message = "Error al obtener actividades obligatorias", error = ex.Message });
            }
        }
    }
}
