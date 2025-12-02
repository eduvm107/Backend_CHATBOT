using System.Text.Json.Serialization;

namespace ChatbotTCS.AdminAPI.Models
{
    // ============================================
    // MODELOS PARA EMBEDDINGS
    // ============================================

    public class EmbeddingRequest
    {
        public string model { get; set; } = "nomic-embed-text"; ///modelo para conversion a vector
        public string prompt { get; set; }
    }

    // repuesta de embedding
    public class EmbeddingResponse
    {
        public double[] embedding { get; set; }
    }

    // ============================================
    // MODELOS PARA CHAT
    // ============================================

    // Solicitud para el Chat (Llama 3.2)
    public class ChatRequest
    {
        public string model { get; set; } = "llama3.2"; // MODELO de chat para respuesta
        public bool stream { get; set; } = false;
        public List<Message> messages { get; set; } = new();
    }

    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ChatResponse
    {
        public Message message { get; set; }
    }

    // ============================================
    // MODELOS PARA GENERATE API
    // ============================================

    /// <summary>
    /// Solicitud para generar respuesta en Ollama usando /api/generate
    /// </summary>
    public class OllamaGenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.6;

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 0.85;

        [JsonPropertyName("top_k")]
        public int TopK { get; set; } = 40;

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 500;
    }

    /// <summary>
    /// Respuesta de Ollama con la respuesta generada
    /// </summary>
    public class OllamaGenerateResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }
    }

    // ============================================
    // MODELOS PARA TAGS/MODELOS
    // ============================================

    /// <summary>
    /// Respuesta de Ollama con lista de modelos
    /// </summary>
    public class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaModel> Models { get; set; } = new List<OllamaModel>();
    }

    /// <summary>
    /// Información de un modelo en Ollama
    /// </summary>
    public class OllamaModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; }

        [JsonPropertyName("size")]
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