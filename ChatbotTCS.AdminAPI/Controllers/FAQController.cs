using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones CRUD de FAQs
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly MongoDBService _mongoDBService;
        private readonly ILogger<FAQController> _logger;

        /// <summary>
        /// Constructor del controlador FAQ
        /// </summary>
        /// <param name="mongoDBService">Servicio de MongoDB</param>
        /// <param name="logger">Logger para registrar eventos</param>
        public FAQController(MongoDBService mongoDBService, ILogger<FAQController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las FAQs
        /// </summary>
        /// <returns>Lista de FAQs</returns>
        /// <response code="200">Retorna la lista de FAQs</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<FAQ>>> GetAll()
        {
            try
            {
                var faqs = await _mongoDBService.GetAllFAQsAsync();
                return Ok(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las FAQs");
                return StatusCode(500, new { message = "Error al obtener las FAQs", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene una FAQ específica por ID
        /// </summary>
        /// <param name="id">ID de la FAQ</param>
        /// <returns>FAQ encontrada</returns>
        /// <response code="200">Retorna la FAQ solicitada</response>
        /// <response code="404">FAQ no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FAQ>> GetById(string id)
        {
            try
            {
                var faq = await _mongoDBService.GetFAQByIdAsync(id);

                if (faq == null)
                {
                    _logger.LogWarning("FAQ no encontrada con ID: {Id}", id);
                    return NotFound(new { message = $"FAQ con ID {id} no encontrada" });
                }

                return Ok(faq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener FAQ con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener la FAQ", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea una nueva FAQ
        /// </summary>
        /// <param name="faq">Datos de la FAQ a crear</param>
        /// <returns>FAQ creada</returns>
        /// <response code="201">FAQ creada exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FAQ>> Create([FromBody] FAQ faq)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear FAQ");
                    return BadRequest(ModelState);
                }

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(faq.Pregunta))
                {
                    return BadRequest(new { message = "La pregunta es requerida" });
                }

                if (string.IsNullOrWhiteSpace(faq.Respuesta))
                {
                    return BadRequest(new { message = "La respuesta es requerida" });
                }

                if (string.IsNullOrWhiteSpace(faq.Categoria))
                {
                    return BadRequest(new { message = "La categoría es requerida" });
                }

                await _mongoDBService.CreateFAQAsync(faq);

                _logger.LogInformation("FAQ creada exitosamente con ID: {Id}", faq.Id);

                // Retornar 201 Created con el location header
                return CreatedAtAction(nameof(GetById), new { id = faq.Id }, faq);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear FAQ");
                return StatusCode(500, new { message = "Error al crear la FAQ", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una FAQ existente
        /// </summary>
        /// <param name="id">ID de la FAQ a actualizar</param>
        /// <param name="faq">Datos actualizados de la FAQ</param>
        /// <returns>Sin contenido si se actualizó exitosamente</returns>
        /// <response code="204">FAQ actualizada exitosamente</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="404">FAQ no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] FAQ faq)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar FAQ");
                    return BadRequest(ModelState);
                }

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(faq.Pregunta))
                {
                    return BadRequest(new { message = "La pregunta es requerida" });
                }

                if (string.IsNullOrWhiteSpace(faq.Respuesta))
                {
                    return BadRequest(new { message = "La respuesta es requerida" });
                }

                if (string.IsNullOrWhiteSpace(faq.Categoria))
                {
                    return BadRequest(new { message = "La categoría es requerida" });
                }

                var updated = await _mongoDBService.UpdateFAQAsync(id, faq);

                if (!updated)
                {
                    _logger.LogWarning("FAQ no encontrada para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"FAQ con ID {id} no encontrada" });
                }

                _logger.LogInformation("FAQ actualizada exitosamente con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar FAQ con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar la FAQ", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una FAQ
        /// </summary>
        /// <param name="id">ID de la FAQ a eliminar</param>
        /// <returns>Sin contenido si se eliminó exitosamente</returns>
        /// <response code="204">FAQ eliminada exitosamente</response>
        /// <response code="404">FAQ no encontrada</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _mongoDBService.DeleteFAQAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("FAQ no encontrada para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"FAQ con ID {id} no encontrada" });
                }

                _logger.LogInformation("FAQ eliminada exitosamente con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar FAQ con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar la FAQ", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca FAQs por texto
        /// </summary>
        /// <param name="query">Texto a buscar en preguntas, respuestas y palabras clave</param>
        /// <returns>Lista de FAQs que coinciden con la búsqueda</returns>
        /// <response code="200">Retorna las FAQs encontradas</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<FAQ>>> Search([FromQuery] string query)
        {
            try
            {
                var faqs = await _mongoDBService.SearchFAQsAsync(query);
                return Ok(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar FAQs con query: {Query}", query);
                return StatusCode(500, new { message = "Error al buscar FAQs", error = ex.Message });
            }
        }
    }
}
