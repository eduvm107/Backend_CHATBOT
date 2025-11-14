using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar documentos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {
        private readonly DocumentoService _documentoService;
        private readonly ILogger<DocumentoController> _logger;

        public DocumentoController(DocumentoService documentoService, ILogger<DocumentoController> logger)
        {
            _documentoService = documentoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los documentos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Documento>>> GetAll()
        {
            try
            {
                var documentos = await _documentoService.GetAllAsync();
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos");
                return StatusCode(500, new { message = "Error al obtener documentos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un documento por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Documento>> GetById(string id)
        {
            try
            {
                var documento = await _documentoService.GetByIdAsync(id);

                if (documento == null)
                {
                    _logger.LogWarning("Documento no encontrado con ID: {Id}", id);
                    return NotFound(new { message = $"Documento con ID {id} no encontrado" });
                }

                return Ok(documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documento con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al obtener documento", error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo documento
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Documento>> Create([FromBody] Documento documento)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear documento");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(documento.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(documento.Url))
                {
                    return BadRequest(new { message = "La URL es requerida" });
                }

                await _documentoService.CreateAsync(documento);

                _logger.LogInformation("Documento creado con ID: {Id}", documento.Id);

                return CreatedAtAction(nameof(GetById), new { id = documento.Id }, documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear documento");
                return StatusCode(500, new { message = "Error al crear documento", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un documento existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] Documento documento)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al actualizar documento");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(documento.Titulo))
                {
                    return BadRequest(new { message = "El título es requerido" });
                }

                if (string.IsNullOrWhiteSpace(documento.Url))
                {
                    return BadRequest(new { message = "La URL es requerida" });
                }

                var updated = await _documentoService.UpdateAsync(id, documento);

                if (!updated)
                {
                    _logger.LogWarning("Documento no encontrado para actualizar con ID: {Id}", id);
                    return NotFound(new { message = $"Documento con ID {id} no encontrado" });
                }

                _logger.LogInformation("Documento actualizado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar documento con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al actualizar documento", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un documento
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deleted = await _documentoService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Documento no encontrado para eliminar con ID: {Id}", id);
                    return NotFound(new { message = $"Documento con ID {id} no encontrado" });
                }

                _logger.LogInformation("Documento eliminado con ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar documento con ID: {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar documento", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene documentos por categoría
        /// </summary>
        [HttpGet("categoria/{categoria}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Documento>>> GetByCategoria(string categoria)
        {
            try
            {
                var documentos = await _documentoService.GetByCategoriaAsync(categoria);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos por categoría: {Categoria}", categoria);
                return StatusCode(500, new { message = "Error al obtener documentos por categoría", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene documentos por tipo
        /// </summary>
        [HttpGet("tipo/{tipo}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Documento>>> GetByTipo(string tipo)
        {
            try
            {
                var documentos = await _documentoService.GetByTipoAsync(tipo);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos por tipo: {Tipo}", tipo);
                return StatusCode(500, new { message = "Error al obtener documentos por tipo", error = ex.Message });
            }
        }

        /// <summary>
        /// Busca documentos por tag
        /// </summary>
        [HttpGet("tag/{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Documento>>> SearchByTag(string tag)
        {
            try
            {
                var documentos = await _documentoService.SearchByTagsAsync(tag);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar documentos por tag: {Tag}", tag);
                return StatusCode(500, new { message = "Error al buscar documentos por tag", error = ex.Message });
            }
        }
    }
}
