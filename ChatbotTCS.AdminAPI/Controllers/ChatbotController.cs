// ============================================
// CHATBOT CONTROLLER
// Archivo: Controllers/ChatbotController.cs
// ============================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using ChatbotTCS.AdminAPI.Services;
using ChatbotTCS.AdminAPI.Models;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para gestionar conversaciones del chatbot
    /// Integra Ollama para generar respuestas con fine-tuning de TCS
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IOllamaService _ollamaService;
        private readonly IMongoCollection<Conversacion> _conversacionesCollection;
        private readonly FAQService _faqService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IOllamaService ollamaService,
            MongoDBService mongoDBService,
            FAQService faqService,
            ILogger<ChatbotController> logger
        )
        {
            _ollamaService = ollamaService;
            _faqService = faqService;
            _logger = logger;

            // Obtener colección de conversaciones desde MongoDBService
            _conversacionesCollection = mongoDBService.GetCollection<Conversacion>("conversaciones");
        }

        // ============================================
        // ENDPOINTS
        // ============================================

        /// <summary>
        /// Envía una pregunta al chatbot y obtiene respuesta
        /// POST /api/chatbot/ask
        /// </summary>
        [HttpPost("ask")]
        public async Task<IActionResult> HacerPregunta([FromBody] ChatbotRequest request)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(request?.Pregunta))
                {
                    return BadRequest(new { error = "La pregunta es requerida" });
                }

                _logger.LogInformation($"📨 Pregunta recibida de usuario {request.UsuarioId}: {request.Pregunta}");

                // 1. Buscar respuestas en FAQs (RAG)
                var contextoFAQ = await BuscarContextoFAQ(request.Pregunta);

                _logger.LogInformation($"🔍 Contexto FAQ encontrado: {(string.IsNullOrEmpty(contextoFAQ) ? "No" : "Sí")}");

                // 2. Generar respuesta con Ollama
                var respuesta = await _ollamaService.GenerarRespuestaAsync(
                    request.Pregunta,
                    contextoFAQ
                );

                _logger.LogInformation($"✅ Respuesta generada exitosamente");

                // 3. Guardar conversación en MongoDB
                var conversacion = new Conversacion
                {
                    UsuarioId = request.UsuarioId,
                    FechaInicio = DateTime.UtcNow,
                    FechaUltimaMensaje = DateTime.UtcNow,
                    Activa = true,
                    Mensajes = new List<Mensaje>
                    {
                        new Mensaje
                        {
                            Tipo = "usuario",
                            Contenido = request.Pregunta,
                            Timestamp = DateTime.UtcNow
                        },
                        new Mensaje
                        {
                            Tipo = "bot",
                            Contenido = respuesta,
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };

                await _conversacionesCollection.InsertOneAsync(conversacion);

                _logger.LogInformation($"💾 Conversación guardada en MongoDB");

                // 4. Retornar respuesta
                return Ok(new ChatbotResponse
                {
                    Respuesta = respuesta,
                    ContextoUtilizado = !string.IsNullOrEmpty(contextoFAQ),
                    FechaRespuesta = DateTime.UtcNow,
                    ConversacionId = conversacion.Id ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en ChatbotController.HacerPregunta: {ex.Message}");
                return StatusCode(500, new { error = "Error interno del servidor", detalles = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el historial de conversaciones de un usuario
        /// GET /api/chatbot/historial/{usuarioId}
        /// </summary>
        [HttpGet("historial/{usuarioId}")]
        public async Task<IActionResult> ObtenerHistorial(string usuarioId)
        {
            try
            {
                var filter = Builders<Conversacion>.Filter.Eq(c => c.UsuarioId, usuarioId);
                var conversaciones = await _conversacionesCollection
                    .Find(filter)
                    .SortByDescending(c => c.FechaInicio)
                    .ToListAsync();

                return Ok(new
                {
                    total = conversaciones.Count,
                    conversaciones = conversaciones
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error obteniendo historial: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Registra satisfacción con la respuesta del chatbot
        /// PUT /api/chatbot/satisfaccion/{conversacionId}
        /// </summary>
        [HttpPut("satisfaccion/{conversacionId}")]
        public async Task<IActionResult> RegistrarSatisfaccion(
            string conversacionId,
            [FromBody] SatisfaccionRequest request
        )
        {
            try
            {
                if (!ObjectId.TryParse(conversacionId, out var objId))
                {
                    return BadRequest("ID de conversación inválido");
                }

                var filter = Builders<Conversacion>.Filter.Eq(c => c.Id, conversacionId);
                var update = Builders<Conversacion>.Update
                    .Set(c => c.Satisfaccion, request.Satisfaccion)
                    .Set(c => c.Resuelto, true);

                var result = await _conversacionesCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation($"✅ Satisfacción registrada para conversación {conversacionId}");
                    return Ok(new { mensaje = "Gracias por tu feedback" });
                }
                else
                {
                    return NotFound("Conversación no encontrada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error registrando satisfacción: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas del chatbot
        /// GET /api/chatbot/estadisticas
        /// </summary>
        [HttpGet("estadisticas")]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                var totalConversaciones = await _conversacionesCollection.CountDocumentsAsync(FilterDefinition<Conversacion>.Empty);

                // Calcula promedio de satisfacción
                var pipeline = new[]
                {
                    new BsonDocument("$match", new BsonDocument("satisfaccion", new BsonDocument("$ne", BsonNull.Value))),
                    new BsonDocument("$group", new BsonDocument
                    {
                        { "_id", BsonNull.Value },
                        { "promedioSatisfaccion", new BsonDocument("$avg", "$satisfaccion") }
                    })
                };

                var result = await _conversacionesCollection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
                var promedioSatisfaccion = result?["promedioSatisfaccion"].AsDouble ?? 0;

                var estadisticas = new
                {
                    totalConversaciones = totalConversaciones,
                    promedioSatisfaccion = Math.Round(promedioSatisfaccion, 2),
                    totalFAQs = await _faqService.CountAsync(),
                    estadoOllama = await _ollamaService.ObtenerInfoModeloAsync()
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error obteniendo estadísticas: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verifica que Ollama esté operativo
        /// GET /api/chatbot/health
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> VerificarSalud()
        {
            try
            {
                var modeloDisponible = await _ollamaService.VerificarModeloAsync();
                var infoOllama = await _ollamaService.ObtenerInfoModeloAsync();

                return Ok(new
                {
                    estado = modeloDisponible ? "activo" : "inactivo",
                    ollama = infoOllama,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================
        // MÉTODOS PRIVADOS
        // ============================================

        /// <summary>
        /// Busca contexto relevante en FAQs usando búsqueda regex
        /// </summary>
        private async Task<string> BuscarContextoFAQ(string pregunta)
        {
            try
            {
                // Usar el método BuscarRelevantesAsync del FAQService
                var faqs = await _faqService.BuscarRelevantesAsync(pregunta);

                if (!faqs.Any())
                {
                    return null;
                }

                // Construir contexto de las FAQs encontradas
                var contexto = "FAQs relevantes encontradas:\n";
                foreach (var faq in faqs)
                {
                    contexto += $"\nPregunta: {faq.Pregunta}\n";
                    contexto += $"Respuesta: {faq.Respuesta}\n";
                }

                return contexto;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Error en búsqueda de FAQs: {ex.Message}");
                return null;
            }
        }

    }

    // ============================================
    // MODELOS DE SOLICITUD/RESPUESTA
    // ============================================

    public class ChatbotRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Pregunta { get; set; } = string.Empty;
    }

    public class ChatbotResponse
    {
        public string Respuesta { get; set; } = string.Empty;
        public bool ContextoUtilizado { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string ConversacionId { get; set; } = string.Empty;
    }

    public class SatisfaccionRequest
    {
        public int Satisfaccion { get; set; } // 1-5
    }
}
