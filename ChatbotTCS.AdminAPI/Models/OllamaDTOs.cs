namespace ChatbotTCS.AdminAPI.Models
{
    
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
}
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