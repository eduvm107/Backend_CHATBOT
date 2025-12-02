using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para métricas del panel de administración
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly MensajeAutomaticoService _mensajeService;
        private readonly ActividadService _actividadService;
        private readonly DocumentoService _documentoService;
        private readonly ConversacionService _conversacionService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(
            MensajeAutomaticoService mensajeService,
            ActividadService actividadService,
            DocumentoService documentoService,
            ConversacionService conversacionService,
            ILogger<MetricsController> logger)
        {
            _mensajeService = mensajeService;
            _actividadService = actividadService;
            _documentoService = documentoService;
            _conversacionService = conversacionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene métricas generales del sistema
        /// GET /api/metrics
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MetricsResponse>> GetMetrics()
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas del sistema");

                // Obtener totales de cada colección
                var mensajes = await _mensajeService.GetAllAsync();
                var actividades = await _actividadService.GetAllAsync();
                var documentos = await _documentoService.GetAllAsync();
                var conversaciones = await _conversacionService.GetAllAsync();

                // Calcular tasa de completitud (actividades obligatorias)
                var actividadesObligatorias = await _actividadService.GetObligatoriasAsync();
                var completionRate = actividadesObligatorias.Any()
                    ? (int)Math.Round((double)actividadesObligatorias.Count / actividades.Count * 100)
                    : 87; // Valor por defecto si no hay datos

                // Calcular satisfacción promedio de conversaciones
                var conversacionesConSatisfaccion = conversaciones
                    .Where(c => c.Satisfaccion.HasValue && c.Satisfaccion.Value > 0)
                    .ToList();

                var averageSatisfaction = conversacionesConSatisfaccion.Any()
                    ? Math.Round(conversacionesConSatisfaccion.Average(c => c.Satisfaccion!.Value), 2)
                    : 4.5; // Valor por defecto

                // Calcular promedio de días (basado en actividades)
                var averageTimeDays = actividades.Any()
                    ? (int)Math.Round(actividades.Average(a => a.Dia))
                    : 14; // Valor por defecto

                var metrics = new MetricsResponse
                {
                    TotalContents = mensajes.Count,
                    TotalActivities = actividades.Count,
                    TotalResources = documentos.Count,
                    CompletionRate = completionRate,
                    AverageSatisfaction = averageSatisfaction,
                    AverageTimeDays = averageTimeDays,
                    ActiveUsers = conversaciones.Count(c => c.Activa),
                    TotalInteractions = conversaciones.Sum(c => c.Mensajes?.Count ?? 0)
                };

                _logger.LogInformation("Métricas calculadas exitosamente");
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas");
                return StatusCode(500, new { message = "Error al obtener métricas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene métricas de conversaciones
        /// GET /api/metrics/conversaciones
        /// </summary>
        [HttpGet("conversaciones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetConversacionesMetrics()
        {
            try
            {
                var conversaciones = await _conversacionService.GetAllAsync();

                var metrics = new
                {
                    total = conversaciones.Count,
                    activas = conversaciones.Count(c => c.Activa),
                    resueltas = conversaciones.Count(c => c.Resuelto),
                    conSatisfaccion = conversaciones.Count(c => c.Satisfaccion.HasValue),
                    promedioSatisfaccion = conversaciones
                        .Where(c => c.Satisfaccion.HasValue)
                        .Average(c => c.Satisfaccion!.Value),
                    totalMensajes = conversaciones.Sum(c => c.Mensajes?.Count ?? 0)
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de conversaciones");
                return StatusCode(500, new { message = "Error al obtener métricas de conversaciones", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene métricas de actividades
        /// GET /api/metrics/actividades
        /// </summary>
        [HttpGet("actividades")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetActividadesMetrics()
        {
            try
            {
                var actividades = await _actividadService.GetAllAsync();
                var obligatorias = await _actividadService.GetObligatoriasAsync();

                var porModalidad = actividades
                    .GroupBy(a => a.Modalidad)
                    .Select(g => new { modalidad = g.Key, count = g.Count() })
                    .ToList();

                var porTipo = actividades
                    .GroupBy(a => a.Tipo)
                    .Select(g => new { tipo = g.Key, count = g.Count() })
                    .ToList();

                var metrics = new
                {
                    total = actividades.Count,
                    obligatorias = obligatorias.Count,
                    porModalidad = porModalidad,
                    porTipo = porTipo,
                    capacidadTotal = actividades.Sum(a => a.CapacidadMaxima)
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de actividades");
                return StatusCode(500, new { message = "Error al obtener métricas de actividades", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene métricas de documentos
        /// GET /api/metrics/documentos
        /// </summary>
        [HttpGet("documentos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetDocumentosMetrics()
        {
            try
            {
                var documentos = await _documentoService.GetAllAsync();

                var porCategoria = documentos
                    .GroupBy(d => d.Categoria)
                    .Select(g => new { categoria = g.Key, count = g.Count() })
                    .ToList();

                var porTipo = documentos
                    .GroupBy(d => d.Tipo)
                    .Select(g => new { tipo = g.Key, count = g.Count() })
                    .ToList();

                var metrics = new
                {
                    total = documentos.Count,
                    obligatorios = documentos.Count(d => d.Obligatorio),
                    porCategoria = porCategoria,
                    porTipo = porTipo,
                    totalAccesos = documentos.Sum(d => d.Accesos ?? 0),
                    totalDescargas = documentos.Sum(d => d.Descargas ?? 0)
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de documentos");
                return StatusCode(500, new { message = "Error al obtener métricas de documentos", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modelo de respuesta para métricas generales
    /// </summary>
    public class MetricsResponse
    {
        public int TotalContents { get; set; }
        public int TotalActivities { get; set; }
        public int TotalResources { get; set; }
        public int CompletionRate { get; set; }
        public double AverageSatisfaction { get; set; }
        public int AverageTimeDays { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalInteractions { get; set; }
    }
}
