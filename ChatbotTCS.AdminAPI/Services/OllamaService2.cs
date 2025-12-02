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

            // Lee sección específica "OllamaRAG"
            _baseUrl = config["OllamaRAG:BaseUrl"] ?? "http://localhost:11434";
            _embeddingModel = config["OllamaRAG:EmbeddingModel"] ?? "mxbai-embed-large";
            _chatModel = config["OllamaRAG:ChatModel"] ?? "llama3.2";
        }

        //  Método para Embeddings (Búsqueda)
        public async Task<double[]> GetEmbeddingAsync(string text)
        {
            var request = new EmbeddingRequest
            {
                model = _embeddingModel, // Usa "mxbai-embed-large" desde config
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

        // 2. Método para Chat Normal (Sin Streaming) - para mensajes cortos o errores
        public async Task<string> ChatAsync(string promptSistema, string preguntaUsuario)
        {
            var request = new ChatRequest
            {
                model = _chatModel, // Usa "llama3.2" desde config
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

        // 3.  MÉTODO: Chat con Streaming (Respuesta gota a gota)
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
                stream = true // <--- Esto activa el modo rápido
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Usamos SendAsync con ResponseHeadersRead para no esperar a que termine todo el texto
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/chat") { Content = jsonContent };
            using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            // Reemplaza el bloque while en ChatStreamAsync para evitar yield dentro de try-catch
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                ChatResponse json = null;
                try
                {
                    // Ollama envía cada palabra en un JSON separado
                    json = JsonSerializer.Deserialize<ChatResponse>(line);
                }
                catch
                {
                    // Ignora errores de deserialización
                }

                // Si hay contenido, lo devolvemos inmediatamente
                if (!string.IsNullOrEmpty(json?.message?.content))
                {
                    yield return json.message.content;
                }
            }
        }
    }
}