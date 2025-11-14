using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para configuración del sistema
    /// </summary>
    public class Configuracion
    {
        /// <summary>
        /// ID único de la configuración
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Tipo de configuración: chatbot, notificaciones, seguridad, general
        /// </summary>
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la configuración
        /// </summary>
        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la configuración
        /// </summary>
        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Objeto de configuración dinámica
        /// </summary>
        [BsonElement("configuracion")]
        public BsonDocument ConfiguracionData { get; set; } = new BsonDocument();

        /// <summary>
        /// Indica si la configuración está activa
        /// </summary>
        [BsonElement("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha de creación
        /// </summary>
        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [BsonElement("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que modificó la configuración
        /// </summary>
        [BsonElement("modificadoPor")]
        public string ModificadoPor { get; set; } = string.Empty;
    }
}
