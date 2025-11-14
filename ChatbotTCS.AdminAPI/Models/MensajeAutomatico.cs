using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para mensajes automáticos del chatbot
    /// </summary>
    public class MensajeAutomatico
    {
        /// <summary>
        /// ID único del mensaje
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Título del mensaje
        /// </summary>
        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Contenido del mensaje
        /// </summary>
        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de mensaje: bienvenida, recordatorio, motivacional, informativo, reenganche
        /// </summary>
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Día en el que se activa el mensaje (nullable)
        /// </summary>
        [BsonElement("diaGatillo")]
        [BsonIgnoreIfNull]
        public int? DiaGatillo { get; set; }

        /// <summary>
        /// Prioridad del mensaje: alta, media, baja
        /// </summary>
        [BsonElement("prioridad")]
        public string Prioridad { get; set; } = string.Empty;

        /// <summary>
        /// Canales de envío: chatbot, email
        /// </summary>
        [BsonElement("canal")]
        public List<string> Canal { get; set; } = new List<string>();

        /// <summary>
        /// Indica si el mensaje está activo
        /// </summary>
        [BsonElement("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Segmento de usuarios: todos, inactivos
        /// </summary>
        [BsonElement("segmento")]
        public string Segmento { get; set; } = string.Empty;

        /// <summary>
        /// Hora de envío en formato HH:mm
        /// </summary>
        [BsonElement("horaEnvio")]
        public string HoraEnvio { get; set; } = string.Empty;

        /// <summary>
        /// Condición para enviar el mensaje (nullable)
        /// </summary>
        [BsonElement("condicion")]
        [BsonIgnoreIfNull]
        public string? Condicion { get; set; }

        /// <summary>
        /// Fecha de creación del mensaje
        /// </summary>
        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó el mensaje
        /// </summary>
        [BsonElement("creadoPor")]
        public string CreadoPor { get; set; } = string.Empty;
    }
}
