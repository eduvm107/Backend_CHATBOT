using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatbotTCS.AdminAPI.Models;

namespace ChatbotTCS.AdminAPI.Services
{
    public class OllamaService2
    {
        private readonly HttpClient _httpClient;

        private readonly string _localOllamaUrl = "http://localhost:11434";
        private readonly string _localEmbeddingModel = "mxbai-embed-large";

        private readonly string _groqUrl;
        private readonly string _groqApiKey;
        private readonly string _groqModel;

        public OllamaService2(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _groqUrl = config["OllamaRAG:BaseUrl"] ?? "https://api.groq.com/openai/v1";
            _groqApiKey = config["OllamaRAG:ApiKey"] ?? "";
            _groqModel = config["OllamaRAG:ChatModel"] ?? "llama3-8b-8192";
        }

        public async Task<double[]> GetEmbeddingAsync(string text)
        {
            var request = new
            {
                model = _localEmbeddingModel,
                prompt = text.Replace("\n", " ")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_localOllamaUrl}/api/embeddings", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                if (doc.RootElement.TryGetProperty("embedding", out var embeddingElement))
                {
                    return JsonSerializer.Deserialize<double[]>(embeddingElement.GetRawText()) ?? Array.Empty<double>();
                }

                return Array.Empty<double>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Embedding: {ex.Message}");
                return Array.Empty<double>();
            }
        }

        public async Task<string> ChatAsync(string promptSistema, string preguntaUsuario)
        {
            var sistema = string.IsNullOrWhiteSpace(promptSistema) ? "Eres un asistente útil." : promptSistema;
            var usuario = string.IsNullOrWhiteSpace(preguntaUsuario) ? "Hola" : preguntaUsuario;

            var requestBody = new
            {
                model = _groqModel,
                messages = new[]
                {
                    new { role = "system", content = sistema },
                    new { role = "user", content = usuario }
                },
                temperature = 0.5
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_groqUrl}/chat/completions");
            request.Content = jsonContent;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error Provider: {response.StatusCode} - {errorContent}";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "Sin respuesta.";
            }
            catch (Exception ex)
            {
                return $"Error de conexión: {ex.Message}";
            }
        }

        public IAsyncEnumerable<string> ChatStreamAsync(string promptSistema, string preguntaUsuario)
        {
            throw new NotImplementedException("Streaming no implementado para este proveedor.");
        }

        public async Task<string> ClasificarIntencionAsync(string preguntaUsuario)
        {
            var promptClasificador = @"
                Eres un clasificador de intenciones para un chatbot corporativo de TCS.
                Analiza con cuidado la pregunta y responde ÚNICAMENTE con una de estas palabras clave:
        
                - ACTIVIDAD: Si el usuario pregunta por su agenda, horario, reuniones, 'qué tengo hoy', fechas o tareas pendientes.
                - PERFIL: Preguntas sobre datos personales del usuario: 'quién es mi supervisor', 'mi jefe', 'mi correo', 'mi puesto', 'mi fecha de ingreso'.
                - RECURSO: El usuario pide explícitamente un archivo, enlace, link, manual, documento, pdf o video. (Ej: 'pásame el link', 'quiero descargar el manual', 'dónde está el formulario').
                - CONSULTA: Oraciones y preguntas generales de cualquier tema (Ej: 'cómo me visto', 'cuántos días de vacaciones tengo', 'qué es el código de conducta').
                - SALUDO: SOLO Saludos, despedidas o agradecimientos simples.
        
                Responde SOLO con la palabra clave (ACTIVIDAD, RECURSO, CONSULTA o SALUDO). No añadas puntuación ni explicaciones.";

            var respuesta = await ChatAsync(promptClasificador, preguntaUsuario);

            return respuesta.Trim().ToUpper().Replace(".", "");
        }
    }
}