using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para documentos del sistema
    /// </summary>
    public class Documento
    {
        /// <summary>
        /// ID único del documento
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Título del documento
        /// </summary>
        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del documento
        /// </summary>
        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// URL del documento
        /// </summary>
        [BsonElement("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de documento: PDF, Formulario Web, Portal Web, Mapa Interactivo
        /// </summary>
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Categoría: Manuales, Políticas, Formularios, Beneficios, Facilidades, Desarrollo
        /// </summary>
        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Subcategoría del documento
        /// </summary>
        [BsonElement("subcategoria")]
        public string Subcategoria { get; set; } = string.Empty;

        /// <summary>
        /// Etiquetas del documento
        /// </summary>
        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Icono emoji del documento
        /// </summary>
        [BsonElement("icono")]
        public string Icono { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño del documento (nullable)
        /// </summary>
        [BsonElement("tamaño")]
        [BsonIgnoreIfNull]
        public string? Tamaño { get; set; }

        /// <summary>
        /// Idioma del documento
        /// </summary>
        [BsonElement("idioma")]
        public string Idioma { get; set; } = string.Empty;

        /// <summary>
        /// Versión del documento
        /// </summary>
        [BsonElement("version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Público objetivo del documento
        /// </summary>
        [BsonElement("publico")]
        public string Publico { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el documento es obligatorio
        /// </summary>
        [BsonElement("obligatorio")]
        public bool Obligatorio { get; set; } = false;

        /// <summary>
        /// Fecha de publicación del documento
        /// </summary>
        [BsonElement("fechaPublicacion")]
        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [BsonElement("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Autor del documento
        /// </summary>
        [BsonElement("autor")]
        public string Autor { get; set; } = string.Empty;

        /// <summary>
        /// Número de descargas (opcional)
        /// </summary>
        [BsonElement("descargas")]
        [BsonIgnoreIfNull]
        public int? Descargas { get; set; }

        /// <summary>
        /// Número de accesos (opcional)
        /// </summary>
        [BsonElement("accesos")]
        [BsonIgnoreIfNull]
        public int? Accesos { get; set; }

        /// <summary>
        /// Valoración del documento
        /// </summary>
        [BsonElement("valoracion")]
        public int Valoracion { get; set; } = 0;
    }
}
