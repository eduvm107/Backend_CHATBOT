using System.ComponentModel.DataAnnotations;

namespace ChatbotTCS.AdminAPI.Models
{
    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email del usuario
        /// </summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es v�lido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contrase�a del usuario
        /// </summary>
        [Required(ErrorMessage = "La contrase�a es requerida")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la solicitud de olvido de contrase�a
    /// </summary>
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es v�lido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la respuesta de forgot-password con token
    /// </summary>
    public class ForgotPasswordResponse
    {
        /// <summary>
        /// Mensaje de respuesta
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Token de restablecimiento (solo para desarrollo/testing)
        /// </summary>
        public string? Token { get; set; }
    }

    /// <summary>
    /// Modelo para verificar el token de restablecimiento
    /// </summary>
    public class VerifyResetTokenRequest
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para restablecer la contrase�a con token
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contrase�a es requerida")]
        [MinLength(6, ErrorMessage = "La contrase�a debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para la respuesta de login exitoso
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Mensaje de respuesta
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Token de autenticaci�n (opcional para futuras implementaciones)
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Informaci�n del usuario autenticado
        /// </summary>
        public UsuarioInfo Usuario { get; set; } = new UsuarioInfo();
    }

    /// <summary>
    /// Modelo para cambiar contraseña cuando el usuario está autenticado
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Informaci�n b�sica del usuario para respuestas
    /// </summary>
    public class UsuarioInfo
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Departamento del usuario
        /// </summary>
        public string Departamento { get; set; } = string.Empty;

        /// <summary>
        /// Puesto del usuario
        /// </summary>
        public string Puesto { get; set; } = string.Empty;

        /// <summary>
        /// Estado activo del usuario
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Estado de verificaci�n del usuario
        /// </summary>
        public bool Verificado { get; set; }

        /// <summary>
        /// Estado del onboarding
        /// </summary>
        public string EstadoOnboarding { get; set; } = string.Empty;

        /// <summary>
        /// Progreso del onboarding
        /// </summary>
        public int ProgresoOnboarding { get; set; }

        /// <summary>
        /// Rol del usuario
        /// </summary>
        public string Rol { get; set; } = string.Empty;
    }
}