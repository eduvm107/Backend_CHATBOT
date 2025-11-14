namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Configuración de conexión a MongoDB
    /// </summary>
    public class MongoDBSettings
    {
        /// <summary>
        /// Cadena de conexión a MongoDB
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la base de datos
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}
