// ============================================
// SERVICIO OLLAMA PARA ASP.NET CORE
// Archivo: Services/OllamaService.cs
// ============================================

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Servicio para comunicarse con Ollama (servidor local de modelos IA)
    /// Permite generar respuestas usando Llama 3.2 fine-tuned para TCS
    /// </summary>
    public interface IOllamaService
    {
        Task<string> GenerarRespuestaAsync(string pregunta, string contexto = "");
        Task<bool> VerificarModeloAsync();
        Task<OllamaInfoResponse> ObtenerInfoModeloAsync();
    }

    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService> _logger;
        private readonly string _ollamaUrl;
        private readonly string _modeloNombre;

        public OllamaService(
            HttpClient httpClient,
            ILogger<OllamaService> logger,
            string ollamaUrl = "http://localhost:11434",
            string modeloNombre = "llama-tcs"
        )
        {
            _httpClient = httpClient;
            _logger = logger;
            _ollamaUrl = ollamaUrl;
            _modeloNombre = modeloNombre;
        }

        /// <summary>
        /// Genera una respuesta usando el modelo Ollama
        /// </summary>
        public async Task<string> GenerarRespuestaAsync(string pregunta, string contexto = "")
        {
            try
            {
                _logger.LogInformation($"🤖 Generando respuesta para: {pregunta}");

                // Construir el prompt con contexto si está disponible
                string prompt = string.IsNullOrEmpty(contexto)
                    ? pregunta
                    : $"Contexto: {contexto}\n\nPregunta: {pregunta}";

                // Crear la solicitud
                var request = new OllamaGenerateRequest
                {
                    Model = _modeloNombre,
                    Prompt = prompt,
                    Stream = false,
                    Temperature = 0.6,
                    TopP = 0.85,
                    TopK = 40
                };

                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                _logger.LogDebug($"📝 Enviando a Ollama: {_ollamaUrl}/api/generate");

                // Enviar a Ollama
                var response = await _httpClient.PostAsync(
                    $"{_ollamaUrl}/api/generate",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ Error de Ollama: {response.StatusCode}");
                    return "Lo siento, tuve un problema técnico. ¿Podrías reformular tu pregunta?";
                }

                // Parsear respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseContent);

                _logger.LogInformation($"✅ Respuesta generada exitosamente");

                return ollamaResponse?.Response ?? "No se pudo generar una respuesta";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"🔌 Error de conexión con Ollama: {ex.Message}");
                return "No puedo conectarme con el servidor. Intenta más tarde.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error inesperado: {ex.Message}");
                return "Ocurrió un error inesperado. Por favor intenta de nuevo.";
            }
        }

        /// <summary>
        /// Verifica si Ollama está disponible y el modelo está cargado
        /// </summary>
        public async Task<bool> VerificarModeloAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Verificando disponibilidad de Ollama...");

                var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(content);

                    bool modeloEncontrado = false;
                    foreach (var modelo in tags?.Models ?? new System.Collections.Generic.List<OllamaModel>())
                    {
                        if (modelo.Name.Contains(_modeloNombre))
                        {
                            modeloEncontrado = true;
                            _logger.LogInformation($"✅ Modelo {_modeloNombre} está disponible");
                            break;
                        }
                    }

                    if (!modeloEncontrado)
                    {
                        _logger.LogWarning($"⚠️ Modelo {_modeloNombre} no encontrado");
                        _logger.LogWarning("💡 Modelos disponibles:");
                        foreach (var modelo in tags?.Models ?? new System.Collections.Generic.List<OllamaModel>())
                        {
                            _logger.LogWarning($"   - {modelo.Name}");
                        }
                    }

                    return modeloEncontrado;
                }

                _logger.LogError($"❌ Ollama no respondió: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al verificar modelo: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene información sobre el modelo
        /// </summary>
        public async Task<OllamaInfoResponse> ObtenerInfoModeloAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(content);

                    return new OllamaInfoResponse
                    {
                        ModeloActual = _modeloNombre,
                        Disponible = true,
                        TotalModelos = tags?.Models?.Count ?? 0,
                        Url = _ollamaUrl
                    };
                }

                return new OllamaInfoResponse
                {
                    ModeloActual = _modeloNombre,
                    Disponible = false,
                    Error = "No se pudo conectar con Ollama"
                };
            }
            catch (Exception ex)
            {
                return new OllamaInfoResponse
                {
                    ModeloActual = _modeloNombre,
                    Disponible = false,
                    Error = ex.Message
                };
            }
        }
    }

    // ============================================
    // MODELOS DE DATOS
    // ============================================

    /// <summary>
    /// Solicitud para generar respuesta en Ollama
    /// </summary>
    public class OllamaGenerateRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("model")]
        public string Model { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [System.Text.Json.Serialization.JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.6;

        [System.Text.Json.Serialization.JsonPropertyName("top_p")]
        public double TopP { get; set; } = 0.85;

        [System.Text.Json.Serialization.JsonPropertyName("top_k")]
        public int TopK { get; set; } = 40;

        [System.Text.Json.Serialization.JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 500;
    }

    /// <summary>
    /// Respuesta de Ollama con la respuesta generada
    /// </summary>
    public class OllamaGenerateResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("model")]
        public string Model { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public string Response { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("done")]
        public bool Done { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }
    }

    /// <summary>
    /// Respuesta de Ollama con lista de modelos
    /// </summary>
    public class OllamaTagsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("models")]
        public List<OllamaModel> Models { get; set; } = new List<OllamaModel>();
    }

    /// <summary>
    /// Información de un modelo en Ollama
    /// </summary>
    public class OllamaModel
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("size")]
        public long Size { get; set; }
    }

    /// <summary>
    /// Información del estado de Ollama
    /// </summary>
    public class OllamaInfoResponse
    {
        public string ModeloActual { get; set; }
        public bool Disponible { get; set; }
        public int TotalModelos { get; set; }
        public string Url { get; set; }
        public string Error { get; set; }
    }
}
