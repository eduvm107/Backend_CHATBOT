namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo unificado para representar recursos favoritos (Documentos, Actividades, Conversaciones)
    /// </summary>
    public class RecursoFavorito
    {
        /// <summary>
        /// ID del recurso favorito
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Tipo de recurso: "documento", "actividad", "chat"
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Título del recurso
        /// </summary>
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del recurso
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// URL del recurso (nullable, para documentos principalmente)
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Fecha relevante del recurso (fecha de publicación, fecha de actividad, etc.)
        /// </summary>
        public DateTime? FechaRelevante { get; set; }
    }
}
