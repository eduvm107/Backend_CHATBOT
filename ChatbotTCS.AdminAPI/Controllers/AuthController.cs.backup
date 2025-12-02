using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatbotTCS.AdminAPI.Controllers
{
    /// <summary>
    /// Controlador para autenticación de usuarios
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;
        private readonly string _jwtSecret = "TuClaveSecretaSuperSegura"; // Cambia esto por una clave segura;

        public AuthController(UsuarioService usuarioService, ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica a un usuario con email y contraseña
        /// </summary>
        /// <param name="request">Datos de login (email y contraseña)</param>
        /// <returns>Información del usuario autenticado</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de login para email: {Email}", request.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido en login para email: {Email}", request.Email);
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Email o contraseña vacíos en login");
                    return BadRequest(new { message = "Email y contraseña son requeridos" });
                }

                // Buscar usuario por email
                var usuario = await _usuarioService.GetByEmailAsync(request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para email: {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                _logger.LogInformation("Usuario encontrado: {Usuario}", usuario);

                // Verificar si el usuario está activo
                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo intentó hacer login: {Email}", request.Email);
                    return Unauthorized(new { message = "Usuario inactivo" });
                }

                // Verificar contraseña (comparación directa - en producción deberías usar hash)
                if (usuario.Contraseña != request.Password)
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Actualizar fechas de login
                usuario.UltimoLogin = DateTime.UtcNow;
                if (usuario.PrimerLogin == null)
                {
                    usuario.PrimerLogin = DateTime.UtcNow;
                    _logger.LogInformation("Primer login registrado para usuario: {Email}", request.Email);
                }

                // Guardar cambios
                await _usuarioService.UpdateAsync(usuario.Id!, usuario);

                _logger.LogInformation("Login exitoso para usuario: {Email}", request.Email);

                // Generar token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, usuario.Id!),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim(ClaimTypes.Role, usuario.Rol)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Crear respuesta
                var response = new LoginResponse
                {
                    Message = "Login exitoso",
                    Token = tokenHandler.WriteToken(token),
                    Usuario = new UsuarioInfo
                    {
                        Id = usuario.Id!,
                        Email = usuario.Email,
                        NombreCompleto = usuario.NombreCompleto,
                        Nombre = usuario.Nombre,
                        Departamento = usuario.Departamento,
                        Puesto = usuario.Puesto,
                        Activo = usuario.Activo,
                        Verificado = usuario.Verificado,
                        EstadoOnboarding = usuario.EstadoOnboarding,
                        ProgresoOnboarding = usuario.ProgresoOnboarding,
                        Rol = usuario.Rol
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno en el login para email: {Email}", request.Email);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // ...existing code...
    }
}