using ChatbotTCS.AdminAPI.Models;
using ChatbotTCS.AdminAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatbotTCS.AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<AuthController> _logger;
        private readonly string _jwtSecret = "TuClaveSecretaSuperSegura";

        public AuthController(UsuarioService usuarioService, ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

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
                    _logger.LogWarning("Modelo invalido en login para email: {Email}", request.Email);
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogWarning("Email o contraseña vacios en login");
                    return BadRequest(new { message = "Email y contraseña son requeridos" });
                }

                var usuario = await _usuarioService.GetByEmailAsync(request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para email: {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales invalidas" });
                }

                _logger.LogInformation("Usuario encontrado: {Usuario}", usuario);

                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo intento hacer login: {Email}", request.Email);
                    return Unauthorized(new { message = "Usuario inactivo" });
                }

                if (usuario.Contraseña != request.Password)
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales invalidas" });
                }

                usuario.UltimoLogin = DateTime.UtcNow;
                if (usuario.PrimerLogin == null)
                {
                    usuario.PrimerLogin = DateTime.UtcNow;
                    _logger.LogInformation("Primer login registrado para usuario: {Email}", request.Email);
                }

                await _usuarioService.UpdateAsync(usuario.Id!, usuario);

                _logger.LogInformation("Login exitoso para usuario: {Email}", request.Email);

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

        [HttpPost("forgot-password")]
        [HttpPost("/Auth/forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ForgotPasswordResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    _logger.LogWarning("Solicitud de forgot-password sin email");
                    return BadRequest(new { message = "Email es requerido" });
                }

                _logger.LogInformation("Solicitud de forgot-password para email: {Email}", request.Email);

                var usuario = await _usuarioService.GetByEmailAsync(request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("No se encontro usuario para forgot-password con email: {Email}", request.Email);
                    return Ok(new ForgotPasswordResponse
                    {
                        Message = "Si el correo existe, recibiras un token de restablecimiento"
                    });
                }

                var resetToken = GenerateSecureToken();

                usuario.ResetPasswordToken = resetToken;
                usuario.ResetPasswordExpires = DateTime.UtcNow.AddHours(1);
                usuario.FechaActualizacion = DateTime.UtcNow;

                await _usuarioService.UpdateAsync(usuario.Id!, usuario);

                _logger.LogInformation("Se genero token de restablecimiento para email: {Email}, expira en 1 hora", request.Email);

                return Ok(new ForgotPasswordResponse
                {
                    Message = "Si el correo existe, recibiras un token de restablecimiento",
                    Token = resetToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando forgot-password para email: {Email}", request?.Email);
                return StatusCode(500, new { message = "Error al procesar la solicitud. Intenta nuevamente." });
            }
        }

        [HttpPost("verify-reset-token")]
        [HttpPost("/Auth/verify-reset-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return BadRequest(new { message = "Token es requerido" });
                }

                var usuario = await _usuarioService.GetByResetTokenAsync(request.Token);

                if (usuario == null || usuario.ResetPasswordExpires == null || usuario.ResetPasswordExpires < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Token invalido o expirado" });
                }

                return Ok(new { message = "Token valido", email = usuario.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando token de restablecimiento");
                return StatusCode(500, new { message = "Error al verificar el token" });
            }
        }

        [HttpPost("reset-password")]
        [HttpPost("/Auth/reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _usuarioService.GetByResetTokenAsync(request.Token);

                if (usuario == null)
                {
                    return BadRequest(new { message = "Token invalido" });
                }

                if (usuario.ResetPasswordExpires == null || usuario.ResetPasswordExpires < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Token expirado" });
                }

                usuario.Contraseña = request.NewPassword;
                usuario.ResetPasswordToken = null;
                usuario.ResetPasswordExpires = null;
                usuario.FechaActualizacion = DateTime.UtcNow;

                await _usuarioService.UpdateAsync(usuario.Id!, usuario);

                _logger.LogInformation("Contraseña restablecida exitosamente para usuario: {Email}", usuario.Email);

                return Ok(new { message = "Contraseña restablecida exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restableciendo contraseña");
                return StatusCode(500, new { message = "Error al restablecer la contraseña" });
            }
        }

        [HttpPost("change-password")]
        [HttpPost("/Auth/change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido en change-password");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Solicitud de cambio de contraseña para email: {Email}", request.Email);

                var usuario = await _usuarioService.GetByEmailAsync(request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado para cambio de contraseña: {Email}", request.Email);
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning("Usuario inactivo intentó cambiar contraseña: {Email}", request.Email);
                    return Unauthorized(new { message = "Usuario inactivo" });
                }

                if (usuario.Contraseña != request.CurrentPassword)
                {
                    _logger.LogWarning("Contraseña actual incorrecta para usuario: {Email}", request.Email);
                    return Unauthorized(new { message = "La contraseña actual es incorrecta" });
                }

                if (request.CurrentPassword == request.NewPassword)
                {
                    _logger.LogWarning("La nueva contraseña es igual a la actual para usuario: {Email}", request.Email);
                    return BadRequest(new { message = "La nueva contraseña debe ser diferente a la actual" });
                }

                usuario.Contraseña = request.NewPassword;
                usuario.FechaActualizacion = DateTime.UtcNow;

                await _usuarioService.UpdateAsync(usuario.Id!, usuario);

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {Email}", request.Email);

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando contraseña para email: {Email}", request?.Email);
                return StatusCode(500, new { message = "Error al cambiar la contraseña. Intenta nuevamente." });
            }
        }

        private string GenerateSecureToken()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
            }
        }
    }
}
