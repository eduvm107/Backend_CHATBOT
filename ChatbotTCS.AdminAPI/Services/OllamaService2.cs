using System.Text;
using System.Text.Json;
using ChatbotTCS.AdminAPI.Models;

namespace ChatbotTCS.AdminAPI.Services
{
    public class OllamaService2
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _embeddingModel;
        private readonly string _chatModel;

        public OllamaService2(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;

            // Leemos la configuración específica "OllamaRAG" del appsettings.json
            _baseUrl = config["OllamaRAG:BaseUrl"] ?? "http://localhost:11434";
            _embeddingModel = config["OllamaRAG:EmbeddingModel"] ?? "mxbai-embed-large";
            _chatModel = config["OllamaRAG:ChatModel"] ?? "llama3.2";
        }

        // 1. MÉTODO: Generar Embeddings (Vectorizar texto para búsqueda)
        public async Task<double[]> GetEmbeddingAsync(string text)
        {
            var request = new EmbeddingRequest
            {
                model = _embeddingModel,
                prompt = text.Replace("\n", " ")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/embeddings", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse);

                return result?.embedding ?? Array.Empty<double>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Embedding ({_embeddingModel}): {ex.Message}");
                return Array.Empty<double>();
            }
        }

        // 2. MÉTODO: Chat Estándar (Sin Streaming - Para uso interno o mensajes cortos)
        public async Task<string> ChatAsync(string promptSistema, string preguntaUsuario)
        {
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

            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat", jsonContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<ChatResponse>(jsonResponse, options);

                return result?.message?.content ?? "Lo siento, no pude conectar con la IA.";
            }
            catch (Exception ex)
            {
                return $"Error en Chat ({_chatModel}): {ex.Message}";
            }
        }

        // 3. MÉTODO: Chat con Streaming (Respuesta en tiempo real para el usuario)
        public async IAsyncEnumerable<string> ChatStreamAsync(string promptSistema, string preguntaUsuario)
        {
            var request = new ChatRequest
            {
                model = _chatModel,
                messages = new List<Message>
                {
                    new Message { role = "system", content = promptSistema },
                    new Message { role = "user", content = preguntaUsuario }
                },
                stream = true // <--- Activa el modo flujo de datos
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat") { Content = jsonContent };
            // ResponseHeadersRead evita esperar a que se descargue todo el contenido
            using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                ChatResponse? json = null;
                try
                {
                    json = JsonSerializer.Deserialize<ChatResponse>(line);
                }
                catch
                {
                    // Ignoramos líneas corruptas o incompletas
                }

                if (!string.IsNullOrEmpty(json?.message?.content))
                {
                    yield return json.message.content;
                }
            }
        }

        // 4. MÉTODO: Clasificador de Intenciones (El "Policía de Tráfico")
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

            // Usamos ChatAsync normal porque queremos una respuesta única y rápida
            var respuesta = await ChatAsync(promptClasificador, preguntaUsuario);

            // Limpiamos la respuesta para asegurar que sea solo la palabra clave
            return respuesta.Trim().ToUpper().Replace(".", "");
        }
    }
}