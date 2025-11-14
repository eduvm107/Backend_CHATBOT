using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para usuarios del sistema
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// ID único del usuario
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellidos del usuario
        /// </summary>
        [BsonElement("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        [BsonElement("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del usuario
        /// </summary>
        [BsonElement("telefono")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// DNI del usuario
        /// </summary>
        [BsonElement("dni")]
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de nacimiento
        /// </summary>
        [BsonElement("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        /// <summary>
        /// Edad del usuario
        /// </summary>
        [BsonElement("edad")]
        public int Edad { get; set; }

        /// <summary>
        /// Género del usuario
        /// </summary>
        [BsonElement("genero")]
        public string Genero { get; set; } = string.Empty;

        /// <summary>
        /// Estado civil del usuario
        /// </summary>
        [BsonElement("estadoCivil")]
        public string EstadoCivil { get; set; } = string.Empty;

        /// <summary>
        /// Dirección del usuario
        /// </summary>
        [BsonElement("direccion")]
        public Direccion Direccion { get; set; } = new Direccion();

        /// <summary>
        /// Área de trabajo
        /// </summary>
        [BsonElement("area")]
        public string Area { get; set; } = string.Empty;

        /// <summary>
        /// Departamento
        /// </summary>
        [BsonElement("departamento")]
        public string Departamento { get; set; } = string.Empty;

        /// <summary>
        /// Puesto de trabajo
        /// </summary>
        [BsonElement("puesto")]
        public string Puesto { get; set; } = string.Empty;

        /// <summary>
        /// Nivel del puesto
        /// </summary>
        [BsonElement("nivel")]
        public string Nivel { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de contrato
        /// </summary>
        [BsonElement("tipoContrato")]
        public string TipoContrato { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de ingreso
        /// </summary>
        [BsonElement("fechaIngreso")]
        public DateTime FechaIngreso { get; set; }

        /// <summary>
        /// Días desde el ingreso
        /// </summary>
        [BsonElement("diasDesdeIngreso")]
        public int DiasDesdeIngreso { get; set; }

        /// <summary>
        /// Información del supervisor
        /// </summary>
        [BsonElement("supervisor")]
        public Supervisor Supervisor { get; set; } = new Supervisor();

        /// <summary>
        /// Estado del proceso de onboarding
        /// </summary>
        [BsonElement("estadoOnboarding")]
        public string EstadoOnboarding { get; set; } = string.Empty;

        /// <summary>
        /// Progreso del onboarding (porcentaje)
        /// </summary>
        [BsonElement("progresoOnboarding")]
        public int ProgresoOnboarding { get; set; }

        /// <summary>
        /// Lista de actividades completadas
        /// </summary>
        [BsonElement("actividadesCompletadas")]
        public List<string> ActividadesCompletadas { get; set; } = new List<string>();

        /// <summary>
        /// Lista de actividades pendientes
        /// </summary>
        [BsonElement("actividadesPendientes")]
        public List<string> ActividadesPendientes { get; set; } = new List<string>();

        /// <summary>
        /// Lista de documentos entregados
        /// </summary>
        [BsonElement("documentosEntregados")]
        public List<string> DocumentosEntregados { get; set; } = new List<string>();

        /// <summary>
        /// Lista de documentos pendientes
        /// </summary>
        [BsonElement("documentosPendientes")]
        public List<string> DocumentosPendientes { get; set; } = new List<string>();

        /// <summary>
        /// Lista de cursos asignados
        /// </summary>
        [BsonElement("cursosAsignados")]
        public List<string> CursosAsignados { get; set; } = new List<string>();

        /// <summary>
        /// Lista de cursos completados
        /// </summary>
        [BsonElement("cursosCompletados")]
        public List<string> CursosCompletados { get; set; } = new List<string>();

        /// <summary>
        /// Lista de certificaciones
        /// </summary>
        [BsonElement("certificaciones")]
        public List<string> Certificaciones { get; set; } = new List<string>();

        /// <summary>
        /// Lista de favoritos del chat
        /// </summary>
        [BsonElement("favoritosChat")]
        public List<string> FavoritosChat { get; set; } = new List<string>();

        /// <summary>
        /// Preferencias del usuario
        /// </summary>
        [BsonElement("preferencias")]
        public Preferencias Preferencias { get; set; } = new Preferencias();

        /// <summary>
        /// Estadísticas del usuario
        /// </summary>
        [BsonElement("estadisticas")]
        public Estadisticas Estadisticas { get; set; } = new Estadisticas();

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        [BsonElement("activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Indica si el usuario está verificado
        /// </summary>
        [BsonElement("verificado")]
        public bool Verificado { get; set; } = false;

        /// <summary>
        /// Fecha del primer login (nullable)
        /// </summary>
        [BsonElement("primerLogin")]
        [BsonIgnoreIfNull]
        public DateTime? PrimerLogin { get; set; }

        /// <summary>
        /// Fecha del último login (nullable)
        /// </summary>
        [BsonElement("ultimoLogin")]
        [BsonIgnoreIfNull]
        public DateTime? UltimoLogin { get; set; }

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [BsonElement("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que creó el registro
        /// </summary>
        [BsonElement("creadoPor")]
        public string CreadoPor { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clase para la dirección del usuario
    /// </summary>
    public class Direccion
    {
        [BsonElement("calle")]
        public string Calle { get; set; } = string.Empty;

        [BsonElement("distrito")]
        public string Distrito { get; set; } = string.Empty;

        [BsonElement("ciudad")]
        public string Ciudad { get; set; } = string.Empty;

        [BsonElement("pais")]
        public string Pais { get; set; } = string.Empty;

        [BsonElement("codigoPostal")]
        public string CodigoPostal { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clase para la información del supervisor
    /// </summary>
    public class Supervisor
    {
        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [BsonElement("puesto")]
        public string Puesto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clase para las preferencias del usuario
    /// </summary>
    public class Preferencias
    {
        [BsonElement("notificaciones")]
        public bool Notificaciones { get; set; } = true;

        [BsonElement("notificacionesEmail")]
        public bool NotificacionesEmail { get; set; } = true;

        [BsonElement("notificacionesPush")]
        public bool NotificacionesPush { get; set; } = true;

        [BsonElement("idioma")]
        public string Idioma { get; set; } = "es";

        [BsonElement("temaOscuro")]
        public bool TemaOscuro { get; set; } = false;
    }

    /// <summary>
    /// Clase para las estadísticas del usuario
    /// </summary>
    public class Estadisticas
    {
        [BsonElement("mensajesEnviados")]
        public int MensajesEnviados { get; set; } = 0;

        [BsonElement("preguntasRealizadas")]
        public int PreguntasRealizadas { get; set; } = 0;

        [BsonElement("documentosDescargados")]
        public int DocumentosDescargados { get; set; } = 0;

        [BsonElement("ultimaInteraccion")]
        [BsonIgnoreIfNull]
        public DateTime? UltimaInteraccion { get; set; }

        [BsonElement("satisfaccionPromedio")]
        [BsonIgnoreIfNull]
        public double? SatisfaccionPromedio { get; set; }
    }
}
