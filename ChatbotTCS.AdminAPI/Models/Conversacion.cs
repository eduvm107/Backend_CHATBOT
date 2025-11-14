using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para conversaciones del chatbot
    /// </summary>
    public class Conversacion
    {
        /// <summary>
        /// ID único de la conversación
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// ID del usuario de la conversación
        /// </summary>
        [BsonElement("usuarioId")]
        public string UsuarioId { get; set; } = string.Empty;

        /// <summary>
        /// Lista de mensajes de la conversación
        /// </summary>
        [BsonElement("mensajes")]
        public List<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

        /// <summary>
        /// Fecha de inicio de la conversación
        /// </summary>
        [BsonElement("fechaInicio")]
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha del último mensaje
        /// </summary>
        [BsonElement("fechaUltimaMensaje")]
        public DateTime FechaUltimaMensaje { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la conversación está activa
        /// </summary>
        [BsonElement("activa")]
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Nivel de satisfacción de la conversación (nullable)
        /// </summary>
        [BsonElement("satisfaccion")]
        [BsonIgnoreIfNull]
        public int? Satisfaccion { get; set; }

        /// <summary>
        /// Indica si la conversación fue resuelta
        /// </summary>
        [BsonElement("resuelto")]
        public bool Resuelto { get; set; } = false;
    }

    /// <summary>
    /// Clase para los mensajes dentro de una conversación
    /// </summary>
    public class Mensaje
    {
        /// <summary>
        /// Tipo de mensaje: usuario, bot
        /// </summary>
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Contenido del mensaje
        /// </summary>
        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora del mensaje
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// FAQ relacionada con el mensaje (nullable)
        /// </summary>
        [BsonElement("faqRelacionada")]
        [BsonIgnoreIfNull]
        public string? FaqRelacionada { get; set; }
    }
}
