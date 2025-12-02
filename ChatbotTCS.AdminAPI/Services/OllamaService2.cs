using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatbotTCS.AdminAPI.Models;

namespace ChatbotTCS.AdminAPI.Services
{
    public class OllamaService2
    {
        private readonly HttpClient _httpClient;

        // Configuración Híbrida:
        private readonly string _localOllamaUrl = "http://localhost:11434"; // Embeddings siempre locales
        private readonly string _localEmbeddingModel = "mxbai-embed-large";

        // Configuración Groq:
        private readonly string _groqUrl;
        private readonly string _groqApiKey;
        private readonly string _groqModel;

        public OllamaService2(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            // Leemos de appsettings
            _groqUrl = config["OllamaRAG:BaseUrl"] ?? "https://api.groq.com/openai/v1";
            _groqApiKey = config["OllamaRAG:ApiKey"] ?? ""; // ¡Asegúrate de ponerla en el JSON!
            _groqModel = config["OllamaRAG:ChatModel"] ?? "llama3-8b-8192";
        }

        // 1. MÉTODO: Generar Embeddings (Vectorizar texto para búsqueda)
        // ESTE SIGUE USANDO TU OLLAMA LOCAL (Rápido y Gratis)
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
                // Apuntamos siempre al Localhost para embeddings
                var response = await _httpClient.PostAsync($"{_localOllamaUrl}/api/embeddings", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Usamos JsonDocument para mayor flexibilidad
                using var doc = JsonDocument.Parse(jsonResponse);
                var embeddingElement = doc.RootElement.GetProperty("embedding");
                return JsonSerializer.Deserialize<double[]>(embeddingElement.GetRawText());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Embedding ({_localEmbeddingModel}): {ex.Message}");
                return Array.Empty<double>();
            }
        }

        // 2. MÉTODO: Chat Estándar (AHORA USA GROQ) 🚀
        public async Task<string> ChatAsync(string promptSistema, string preguntaUsuario)
        {
            // Validaciones de seguridad para Groq
            var sistema = string.IsNullOrWhiteSpace(promptSistema) ? "Eres un asistente útil." : promptSistema;
            var usuario = string.IsNullOrWhiteSpace(preguntaUsuario) ? "Hola" : preguntaUsuario;

            // Formato compatible con OpenAI/Groq
            var requestBody = new
            {
                model = _groqModel,
                messages = new[]
                {
                    new { role = "system", content = sistema },
                    new { role = "user", content = usuario }
                },
                temperature = 0.5 // Creatividad balanceada
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Creamos la petición HTTP apuntando a Groq
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_groqUrl}/chat/completions");
            request.Content = jsonContent;

            // ¡IMPORTANTE! Aquí va tu clave API de Groq
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Error Groq: {response.StatusCode} - {errorContent}";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Parseamos la respuesta de Groq
                using var doc = JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "Sin respuesta de Groq.";
            }
            catch (Exception ex)
            {
                return $"Error de Conexión Groq: {ex.Message}";
            }
        }

        // 3. MÉTODO: Chat con Streaming (DESHABILITADO PARA GROQ POR AHORA)
        // Lo dejamos lanzando excepción para evitar errores si el controlador intenta usarlo
        public IAsyncEnumerable<string> ChatStreamAsync(string promptSistema, string preguntaUsuario)
        {
            throw new NotImplementedException("El streaming con Groq requiere otra implementación. Usa el modo normal.");
        }

        // 4. MÉTODO: Clasificador de Intenciones (TU LÓGICA ORIGINAL)
        // Sigue usando IA, pero ahora es rapidísimo porque usa Groq
        public async Task<string> ClasificarIntencionAsync(string preguntaUsuario)
        {
            var promptClasificador = @"
                Eres un clasificador de intenciones para un chatbot corporativo de TCS.
                Analiza la pregunta y responde ÚNICAMENTE con una de estas palabras clave:
        
                - ACTIVIDAD: Si el usuario pregunta por su agenda, horario, reuniones, 'qué tengo hoy', fechas o tareas pendientes.
                - PERFIL: Preguntas sobre datos personales del usuario: 'quién es mi supervisor', 'mi jefe', 'mi correo', 'mi puesto', 'mi fecha de ingreso'.
                - RECURSO: El usuario pide explícitamente un archivo, enlace, link, manual, documento, pdf o video. (Ej: 'pásame el link', 'quiero descargar el manual', 'dónde está el formulario').
                - CONSULTA: Preguntas generales donde el usuario quiere una EXPLICACIÓN o respuesta teórica sobre la empresa (Ej: 'cómo me visto', 'cuántos días de vacaciones tengo', 'qué es el código de conducta').
                - SALUDO: Saludos, despedidas o agradecimientos simples.
        
                Responde SOLO con la palabra clave (ACTIVIDAD, RECURSO, CONSULTA o SALUDO). No añadas puntuación ni explicaciones.";

            // Llamamos a ChatAsync (que ahora es Groq)
            var respuesta = await ChatAsync(promptClasificador, preguntaUsuario);

            // Limpiamos la respuesta
            return respuesta.Trim().ToUpper().Replace(".", "");
        }
    }
}