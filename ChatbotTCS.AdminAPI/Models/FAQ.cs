using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo que representa una pregunta frecuente (FAQ) en MongoDB
    /// </summary>
    public class FAQ
    {
        /// <summary>
        /// Identificador único del documento en MongoDB
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Pregunta principal del FAQ
        /// </summary>
        [BsonElement("pregunta")]
        [BsonRequired]
        public string Pregunta { get; set; } = string.Empty;

        /// <summary>
        /// Respuesta breve a la pregunta
        /// </summary>
        [BsonElement("respuesta")]
        [BsonRequired]
        public string Respuesta { get; set; } = string.Empty;

        /// <summary>
        /// Categoría principal del FAQ
        /// </summary>
        [BsonElement("categoria")]
        [BsonRequired]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Subcategoría del FAQ
        /// </summary>
        [BsonElement("subcategoria")]
        public string? Subcategoria { get; set; }

        /// <summary>
        /// Lista de palabras clave para búsqueda
        /// </summary>
        [BsonElement("palabrasClave")]
        public List<string> PalabrasClave { get; set; } = new List<string>();

        /// <summary>
        /// Nivel de prioridad del FAQ (alta, media, baja)
        /// </summary>
        [BsonElement("prioridad")]
        public string? Prioridad { get; set; }

        /// <summary>
        /// Indica si el FAQ está activo
        /// </summary>
        [BsonElement("activa")]
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Contador de veces que se ha usado este FAQ
        /// </summary>
        [BsonElement("vecesUsada")]
        public int VecesUsada { get; set; } = 0;

        /// <summary>
        /// Calificación promedio del FAQ
        /// </summary>
        [BsonElement("rating")]
        public double Rating { get; set; } = 0;

        /// <summary>
        /// Fecha de creación del FAQ
        /// </summary>
        [BsonElement("fechaCreacion")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [BsonElement("fechaActualizacion")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó el FAQ
        /// </summary>
        [BsonElement("creadoPor")]
        public string? CreadoPor { get; set; }

        /// <summary>
        /// Respuesta detallada o extendida
        /// </summary>
        [BsonElement("respuestaLarga")]
        public string? RespuestaLarga { get; set; }

        /// <summary>
        /// Lista de IDs de documentos relacionados
        /// </summary>
        [BsonElement("documentosRelacionados")]
        public List<string> DocumentosRelacionados { get; set; } = new List<string>();

        /// <summary>
        /// Lista de IDs de actividades relacionadas
        /// </summary>
        [BsonElement("actividadesRelacionadas")]
        public List<string> ActividadesRelacionadas { get; set; } = new List<string>();
    }
}
