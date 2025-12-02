// ============================================
// SERVICIO OLLAMA COMPLETO PARA ASP.NET CORE
// Archivo: Services/OllamaService2.cs
// Combina funcionalidad de embeddings, chat y generación
// ============================================

using System.Text;
using System.Text.Json;
using ChatbotTCS.AdminAPI.Models;
using Microsoft.Extensions.Logging;

namespace ChatbotTCS.AdminAPI.Services
{
    /// <summary>
    /// Interfaz del servicio Ollama con todas las funcionalidades
    /// </summary>
    public interface IOllamaService
    {
        // Métodos de Chat y Generación
        Task<string> GenerarRespuestaAsync(string pregunta, string contexto = "");
        Task<string> ChatAsync(string promptSistema, string preguntaUsuario);
        IAsyncEnumerable<string> ChatStreamAsync(string promptSistema, string preguntaUsuario);

        // Métodos de Embeddings
        Task<double[]> GetEmbeddingAsync(string text);

        // Métodos de Gestión de Modelos
        Task<bool> VerificarModeloAsync();
        Task<OllamaInfoResponse> ObtenerInfoModeloAsync();
    }

    /// <summary>
    /// Servicio completo para comunicarse con Ollama (servidor local de modelos IA)
    /// Soporta: Embeddings, Chat, Generación de texto y gestión de modelos
    /// </summary>
    public class OllamaService2 : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OllamaService2> _logger;
        private readonly string _baseUrl;
        private readonly string _embeddingModel;
        private readonly string _chatModel;

        public OllamaService2(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<OllamaService2> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Lee sección específica "OllamaRAG"
            _baseUrl = config["OllamaRAG:BaseUrl"] ?? "http://localhost:11434";
            _embeddingModel = config["OllamaRAG:EmbeddingModel"] ?? "mxbai-embed-large";
            _chatModel = config["OllamaRAG:ChatModel"] ?? "llama3.2";

            _logger.LogInformation($"🔧 Ollama configurado: {_baseUrl}");
            _logger.LogInformation($"📊 Modelo Embedding: {_embeddingModel}");
            _logger.LogInformation($"💬 Modelo Chat: {_chatModel}");
        }

        // ============================================
        // MÉTODOS DE EMBEDDINGS (Para RAG/Búsqueda)
        // ============================================

        /// <summary>
        /// Genera embeddings (vectores) para búsqueda semántica
        /// </summary>
        public async Task<double[]> GetEmbeddingAsync(string text)
        {
            try
            {
                _logger.LogDebug($"🔍 Generando embedding para texto de {text.Length} caracteres");

                var request = new EmbeddingRequest
                {
                    model = _embeddingModel,
                    prompt = text.Replace("\n", " ")
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/embeddings", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse);

                _logger.LogInformation($"✅ Embedding generado exitosamente");
                return result?.embedding ?? Array.Empty<double>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en Embedding ({_embeddingModel}): {ex.Message}");
                return Array.Empty<double>();
            }
        }

        // ============================================
        // MÉTODOS DE CHAT
        // ============================================

        /// <summary>
        /// Chat normal sin streaming - para mensajes cortos
        /// </summary>
        public async Task<string> ChatAsync(string promptSistema, string preguntaUsuario)
        {
            try
            {
                _logger.LogInformation($"💬 Chat: {preguntaUsuario.Substring(0, Math.Min(50, preguntaUsuario.Length))}...");

                var request = new ChatRequest
                {
                    model = _chatModel,
                    messages = new List<Message>
                    {
                        new Message { role = "system", content = promptSistema },
                        new Message { role = "user", content = preguntaUsuario }
                    },
                    stream = false
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ChatResponse>(jsonResponse, options);

                _logger.LogInformation($"✅ Respuesta de chat recibida");
                return result?.message?.content ?? "Lo siento, no pude conectar con la IA.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error en Chat ({_chatModel}): {ex.Message}");
                return $"Error en Chat: {ex.Message}";
            }
        }

        /// <summary>
        /// Chat con streaming - respuesta en tiempo real palabra por palabra
        /// </summary>
        public async IAsyncEnumerable<string> ChatStreamAsync(string promptSistema, string preguntaUsuario)
        {
            _logger.LogInformation($"🌊 Chat Stream iniciado");

            var request = new ChatRequest
            {
                model = _chatModel,
                messages = new List<Message>
                {
                    new Message { role = "system", content = promptSistema },
                    new Message { role = "user", content = preguntaUsuario }
                },
                stream = true
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat")
            {
                Content = jsonContent
            };

            using var response = await _httpClient.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                ChatResponse json = null;
                try
                {
                    json = JsonSerializer.Deserialize<ChatResponse>(line);
                }
                catch
                {
                    // Ignora errores de deserialización
                }

                if (!string.IsNullOrEmpty(json?.message?.content))
                {
                    yield return json.message.content;
                }
            }

            _logger.LogInformation($"✅ Chat Stream completado");
        }

        // ============================================
        // MÉTODO DE GENERACIÓN (Alternativo al Chat)
        // ============================================

        /// <summary>
        /// Genera una respuesta usando el API /generate de Ollama
        /// Útil para prompts simples sin conversación
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

                var request = new OllamaGenerateRequest
                {
                    Model = _chatModel,
                    Prompt = prompt,
                    Stream = false,
                    Temperature = 0.4,  // Reducido para respuestas más concisas y precisas
                    TopP = 0.85,
                    TopK = 40,
                    NumPredict = 150    // Limitado a ~100-120 palabras para respuestas cortas
                };

                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                _logger.LogDebug($"📝 Enviando a Ollama: {_baseUrl}/api/generate");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"❌ Error de Ollama: {response.StatusCode}");
                    return "Lo siento, tuve un problema técnico. ¿Podrías reformular tu pregunta?";
                }

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

        // ============================================
        // MÉTODOS DE GESTIÓN DE MODELOS
        // ============================================

        /// <summary>
        /// Verifica si Ollama está disponible y los modelos están cargados
        /// </summary>
        public async Task<bool> VerificarModeloAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Verificando disponibilidad de Ollama...");

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(content);

                    bool chatModelEncontrado = false;
                    bool embeddingModelEncontrado = false;

                    foreach (var modelo in tags?.Models ?? new List<OllamaModel>())
                    {
                        if (modelo.Name.Contains(_chatModel))
                        {
                            chatModelEncontrado = true;
                            _logger.LogInformation($"✅ Modelo de chat {_chatModel} está disponible");
                        }
                        if (modelo.Name.Contains(_embeddingModel))
                        {
                            embeddingModelEncontrado = true;
                            _logger.LogInformation($"✅ Modelo de embedding {_embeddingModel} está disponible");
                        }
                    }

                    if (!chatModelEncontrado)
                    {
                        _logger.LogWarning($"⚠️ Modelo de chat {_chatModel} no encontrado");
                    }

                    if (!embeddingModelEncontrado)
                    {
                        _logger.LogWarning($"⚠️ Modelo de embedding {_embeddingModel} no encontrado");
                    }

                    if (!chatModelEncontrado || !embeddingModelEncontrado)
                    {
                        _logger.LogWarning("💡 Modelos disponibles:");
                        foreach (var modelo in tags?.Models ?? new List<OllamaModel>())
                        {
                            _logger.LogWarning($"   - {modelo.Name} ({modelo.Size / 1024 / 1024 / 1024} GB)");
                        }
                    }

                    return chatModelEncontrado && embeddingModelEncontrado;
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
        /// Obtiene información completa sobre los modelos configurados
        /// </summary>
        public async Task<OllamaInfoResponse> ObtenerInfoModeloAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(content);

                    return new OllamaInfoResponse
                    {
                        ModeloActual = $"Chat: {_chatModel}, Embedding: {_embeddingModel}",
                        Disponible = true,
                        TotalModelos = tags?.Models?.Count ?? 0,
                        Url = _baseUrl
                    };
                }

                return new OllamaInfoResponse
                {
                    ModeloActual = $"Chat: {_chatModel}, Embedding: {_embeddingModel}",
                    Disponible = false,
                    Error = "No se pudo conectar con Ollama"
                };
            }
            catch (Exception ex)
            {
                return new OllamaInfoResponse
                {
                    ModeloActual = $"Chat: {_chatModel}, Embedding: {_embeddingModel}",
                    Disponible = false,
                    Error = ex.Message
                };
            }
        }
    }
}
