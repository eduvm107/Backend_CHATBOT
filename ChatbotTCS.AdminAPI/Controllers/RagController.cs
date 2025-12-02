using System.Text.Json;
using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotTCS.AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RagController : ControllerBase
    {
        private readonly OllamaService2 _ollamaService;
        private readonly RagMongoService _mongoService;

        public RagController(OllamaService2 ollamaService, RagMongoService mongoService)
        {
            _ollamaService = ollamaService;
            _mongoService = mongoService;
        }

        //  MÉTODO 1: ENTRENAR 
        [HttpPost("entrenar")]
        public async Task<IActionResult> Entrenar([FromBody] string texto, [FromQuery] string categoria)
        {
            var vector = await _ollamaService.GetEmbeddingAsync(texto);
            if (vector.Length == 0) return BadRequest("Error conectando con Ollama.");

            var nuevoDato = new KnowledgeBase
            {
                Content = texto,
                Category = categoria,
                Embedding = vector
            };

            await _mongoService.AddKnowledgeAsync(nuevoDato);
            return Ok("¡Información aprendida exitosamente!");
        }

        // --- MÉTODO 2: CHAT NORMAL  ---
        // RUTA: GET /api/Rag/chat
        [HttpGet("chat")]
        public async Task<IActionResult> ChatNormal([FromQuery] string pregunta)
        {
            var vectorPregunta = await _ollamaService.GetEmbeddingAsync(pregunta);

            var resultados = new List<KnowledgeBase>();
            if (vectorPregunta != null && vectorPregunta.Length > 0)
            {
                resultados = await _mongoService.SearchAsync(vectorPregunta);
            }

            // Filtro de seguridad
            if (resultados.Count == 0 || resultados.First().Score < 0.8)
            {
                var promptDesconocido = @"
                    Eres TCS Assistant, el asistente de RRHH de Tata Consultancy Services.
                    Instrucciones: Discúlpate amablemente, di que solo sabes de Onboarding/Cultura TCS y pide otra pregunta.
                    Tono: Profesional y empático. Párrafo < 50 palabras.";

                var respuestaAmable = await _ollamaService.ChatAsync(promptDesconocido, pregunta); //  ChatAsync normal
                return Ok(new { respuesta = respuestaAmable, fuentes = new List<object>() });
            }

            // Lógica RAG
            var contexto = string.Join("\n\n", resultados.Select(r => r.Content));
            var promptSistema = $@"
                Eres TCS Assistant, experto en RRHH.
                Responde basándote SOLO en este contexto. Máximo 80 palabras.
                {contexto}";

            var respuestaFinal = await _ollamaService.ChatAsync(promptSistema, pregunta); //  ChatAsync normal

            return Ok(new { respuesta = respuestaFinal, fuentes = resultados });
        }

        // --- MÉTODO 3: CHAT STREAMING (Velocidad Real)
        // RUTA: GET /api/Rag/chat-stream
        [HttpGet("chat-stream")]
        public async Task ChatConStreaming([FromQuery] string pregunta)
        {
            Response.ContentType = "text/plain";
            var vectorPregunta = await _ollamaService.GetEmbeddingAsync(pregunta);

            var resultados = new List<KnowledgeBase>();
            if (vectorPregunta != null && vectorPregunta.Length > 0)
            {
                resultados = await _mongoService.SearchAsync(vectorPregunta);
            }

            // Filtro de seguridad Streaming
            if (resultados.Count == 0 || resultados.First().Score < 0.8)
            {
                Response.Headers.Append("X-Fuentes", "[]");
                var promptDesconocido = @"
                    Eres TCS Assistant. El usuario pregunta algo fuera de tema.
                    Instrucciones: Discúlpate, di que solo sabes de Onboarding TCS. Sé breve (< 50 palabras).";

                await foreach (var fragmento in _ollamaService.ChatStreamAsync(promptDesconocido, pregunta))
                {
                    await Response.WriteAsync(fragmento);
                    await Response.Body.FlushAsync();
                }
                return;
            }

            // Lógica RAG Streaming
            var fuentesJson = JsonSerializer.Serialize(resultados);
            Response.Headers.Append("X-Fuentes", fuentesJson.Replace("\n", " ").Replace("\r", ""));

            var contexto = string.Join("\n\n", resultados.Select(r => r.Content));
            var promptSistema = $@"
                Eres TCS Assistant. Responde SOLO con el contexto. Máximo 80 palabras.
                {contexto}";

            await foreach (var fragmento in _ollamaService.ChatStreamAsync(promptSistema, pregunta))
            {
                await Response.WriteAsync(fragmento);
                await Response.Body.FlushAsync();
            }
        }
    }
}
