using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para actividades de onboarding
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Actividad
    {
        /// <summary>
        /// ID único de la actividad
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Título de la actividad
        /// </summary>
        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la actividad
        /// </summary>
        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Día en que se realiza la actividad
        /// </summary>
        [BsonElement("dia")]
        public int Dia { get; set; }

        /// <summary>
        /// Duración en horas de la actividad
        /// </summary>
        [BsonElement("duracionHoras")]
        public double DuracionHoras { get; set; }

        /// <summary>
        /// Hora de inicio en formato HH:mm
        /// </summary>
        [BsonElement("horaInicio")]
        public string HoraInicio { get; set; } = string.Empty;

        /// <summary>
        /// Hora de fin en formato HH:mm
        /// </summary>
        [BsonElement("horaFin")]
        public string HoraFin { get; set; } = string.Empty;

        /// <summary>
        /// Lugar donde se realiza la actividad
        /// </summary>
        [BsonElement("lugar")]
        public string Lugar { get; set; } = string.Empty;

        /// <summary>
        /// Modalidad: presencial, virtual, hibrido, flexible
        /// </summary>
        [BsonElement("modalidad")]
        public string Modalidad { get; set; } = string.Empty;

        /// <summary>
        /// Tipo: induccion, logistica, capacitacion, reunion, evaluacion, taller, integracion
        /// </summary>
        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Categoría de la actividad
        /// </summary>
        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del responsable
        /// </summary>
        [BsonElement("responsable")]
        public string Responsable { get; set; } = string.Empty;

        /// <summary>
        /// Email del responsable (nullable)
        /// </summary>
        [BsonElement("emailResponsable")]
        [BsonIgnoreIfNull]
        public string? EmailResponsable { get; set; }

        /// <summary>
        /// Capacidad máxima de participantes
        /// </summary>
        [BsonElement("capacidadMaxima")]
        public int CapacidadMaxima { get; set; }

        /// <summary>
        /// Indica si la actividad es obligatoria
        /// </summary>
        [BsonElement("obligatorio")]
        public bool Obligatorio { get; set; } = false;

        /// <summary>
        /// Materiales necesarios que debe traer el participante
        /// </summary>
        [BsonElement("materialesNecesarios")]
        public List<string> MaterialesNecesarios { get; set; } = new();

        /// <summary>
        /// Materiales que se proporcionan en la actividad
        /// </summary>
        [BsonElement("materialesProporcionados")]
        public List<string> MaterialesProporcionados { get; set; } = new();

        /// <summary>
        /// Preparación previa necesaria (nullable)
        /// </summary>
        [BsonElement("preparacionPrevia")]
        [BsonIgnoreIfNull]
        public string? PreparacionPrevia { get; set; }

        /// <summary>
        /// Lista de IDs de actividades siguientes
        /// </summary>
        [BsonElement("actividadesSiguientes")]
        public List<string> ActividadesSiguientes { get; set; } = new();

        /// <summary>
        /// Estado de la actividad
        /// </summary>
        [BsonElement("estado")]
        public string Estado { get; set; } = "activo";

        /// <summary>
        /// Fecha de creación de la actividad
        /// </summary>
        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de la actividad (campo en Mongo: fecha_de_actividad)
        /// </summary>
        [BsonElement("fecha_de_actividad")]
        public DateTime FechaDeActividad { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID del usuario asignado a la actividad.
        /// En Mongo se guarda como 'UsuarioID' (ObjectId), en JSON se expone como 'usuarioId'.
        /// </summary>
        [BsonElement("UsuarioID")]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("usuarioId")]
        public string? UsuarioId { get; set; }

        /// <summary>
        /// Indica si la actividad es favorita para el usuario
        /// </summary>
        [BsonElement("favorito")]
        public bool Favorito { get; set; } = false;
    }
}
