// ============================================
// SERVICIO DE PRECARGA DE OLLAMA
// Archivo: Services/OllamaWarmupService.cs
// Precarga el modelo de Ollama al iniciar el backend
// ============================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio que precarga el modelo de Ollama en memoria al iniciar el backend
    /// Esto elimina la espera de 60-90s en la primera petición del usuario
    /// </summary>
    public class OllamaWarmupService : IHostedService
    {
        private readonly IOllamaService _ollamaService;
        private readonly ILogger<OllamaWarmupService> _logger;

        public OllamaWarmupService(
            IOllamaService ollamaService,
            ILogger<OllamaWarmupService> logger)
        {
            _ollamaService = ollamaService;
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta automáticamente al iniciar el backend
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔥 Iniciando precarga de modelo Ollama...");
            _logger.LogInformation("⏰ Esto puede tardar 60-90 segundos en la primera carga");

            try
            {
                var startTime = DateTime.UtcNow;

                // Verificar que Ollama esté disponible
                var modeloDisponible = await _ollamaService.VerificarModeloAsync();

                if (!modeloDisponible)
                {
                    _logger.LogWarning("⚠️ Ollama no está disponible. El modelo se cargará en la primera petición.");
                    return;
                }

                // Hacer una petición dummy para cargar el modelo en memoria
                var promptSistema = "Eres TCS Assistant, un asistente de Tata Consultancy Services.";
                var preguntaDummy = "Hola";

                _logger.LogInformation("📡 Enviando petición de warmup a Ollama...");

                var respuesta = await _ollamaService.ChatAsync(promptSistema, preguntaDummy);

                var tiempoTranscurrido = (DateTime.UtcNow - startTime).TotalSeconds;

                _logger.LogInformation($"✅ Modelo precargado exitosamente en {tiempoTranscurrido:F1} segundos");
                _logger.LogInformation("🚀 El chatbot está listo para responder peticiones rápidamente");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al precargar modelo: {ex.Message}");
                _logger.LogWarning("⚠️ El modelo se cargará en la primera petición del usuario");
            }
        }

        /// <summary>
        /// Se ejecuta al detener el backend (no hace nada)
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Servicio de warmup detenido");
            return Task.CompletedTask;
        }
    }
}
