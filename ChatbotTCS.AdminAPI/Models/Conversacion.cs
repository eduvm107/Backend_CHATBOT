using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    [BsonIgnoreExtraElements] // ¡Vital para evitar errores futuros!
    public class Conversacion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("usuarioId")]
        // Si usas IDs como string en tu app, esto está perfecto:
        public string UsuarioId { get; set; } = string.Empty;

        [BsonElement("mensajes")]
        public List<Mensaje> Mensajes { get; set; } = new List<Mensaje>();

        [BsonElement("fechaInicio")]
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

        [BsonElement("fechaUltimaMensaje")]
        public DateTime FechaUltimaMensaje { get; set; } = DateTime.UtcNow;

        [BsonElement("activa")]
        public bool Activa { get; set; } = true;

        [BsonElement("satisfaccion")]
        [BsonIgnoreIfNull]
        public int? Satisfaccion { get; set; }

        [BsonElement("resuelto")]
        public bool Resuelto { get; set; } = false;


        // Agregado porque estaba en tu JSON
        /// <summary>
        /// Indica si la conversación es un favorito
        /// </summary>
        [BsonElement("favorito")]
        public bool Favorito { get; set; } = false;
    }

    [BsonIgnoreExtraElements]
    public class Mensaje
    {
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty; // "usuario" o "bot"

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // --- EXTRAS ÚTILES ---
        
        [BsonElement("intencionDetectada")]
        [BsonIgnoreIfNull]
        public string? IntencionDetectada { get; set; } // Ej: "ACTIVIDAD"

        [BsonElement("fuentesUsadas")] 
        [BsonIgnoreIfNull]
        public List<string>? FuentesUsadas { get; set; } // Para saber qué docs leyó
    }
}