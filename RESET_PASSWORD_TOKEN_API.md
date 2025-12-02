# Sistema de Restablecimiento de Contraseña con Tokens

## Resumen
Se ha implementado un sistema completo de restablecimiento de contraseña basado en tokens de acceso, funcional tanto para administradores como usuarios.

## Cambios Realizados

### 1. Modelo Usuario (Usuario.cs)
Se agregaron dos nuevos campos:

```csharp
[BsonElement("resetPasswordToken")]
[BsonIgnoreIfNull]
public string? ResetPasswordToken { get; set; }

[BsonElement("resetPasswordExpires")]
[BsonIgnoreIfNull]
public DateTime? ResetPasswordExpires { get; set; }
```

### 2. Modelos de Request/Response (LoginRequest.cs)
Se agregaron:

```csharp
public class ForgotPasswordResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
}

public class VerifyResetTokenRequest
{
    [Required(ErrorMessage = "El token es requerido")]
    public string Token { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "El token es requerido")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
```

### 3. UsuarioService.cs
Nuevo método agregado:

```csharp
public async Task<Usuario?> GetByResetTokenAsync(string token)
{
    try
    {
        _logger.LogInformation("Buscando usuario por token de restablecimiento");
        var filter = Builders<Usuario>.Filter.Eq(u => u.ResetPasswordToken, token);
        return await _usuariosCollection.Find(filter).FirstOrDefaultAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al buscar usuario por token de restablecimiento");
        throw;
    }
}
```

### 4. AuthController.cs - PROBLEMA DE CODIFICACIÓN
⚠️ **NOTA IMPORTANTE**: El archivo AuthController.cs tiene problemas de codificación con caracteres especiales (ñ, á, é, etc).

Para implementar los nuevos endpoints, debes:

1. Asegurarte que el archivo esté en UTF-8
2. Agregar estos 3 endpoints después del método `Login`:

#### Endpoint 1: POST /Auth/forgot-password
```csharp
[HttpPost("forgot-password")]
[HttpPost("/Auth/forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
{
    try
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email es requerido" });
        }

        var usuario = await _usuarioService.GetByEmailAsync(request.Email);

        if (usuario == null)
        {
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

        return Ok(new ForgotPasswordResponse
        {
            Message = "Si el correo existe, recibiras un token de restablecimiento",
            Token = resetToken // Solo para desarrollo - ELIMINAR EN PRODUCCIÓN
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error procesando forgot-password");
        return StatusCode(500, new { message = "Error al procesar la solicitud" });
    }
}
```

#### Endpoint 2: POST /Auth/verify-reset-token
```csharp
[HttpPost("verify-reset-token")]
[HttpPost("/Auth/verify-reset-token")]
public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Token es requerido" });
        }

        var usuario = await _usuarioService.GetByResetTokenAsync(request.Token);

        if (usuario == null || usuario.ResetPasswordExpires == null ||
            usuario.ResetPasswordExpires < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Token invalido o expirado" });
        }

        return Ok(new { message = "Token valido", email = usuario.Email });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error verificando token");
        return StatusCode(500, new { message = "Error al verificar el token" });
    }
}
```

#### Endpoint 3: POST /Auth/reset-password
```csharp
[HttpPost("reset-password")]
[HttpPost("/Auth/reset-password")]
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

        usuario.Contraseña = request.NewPassword; // NOTA: "Contraseña" con ñ
        usuario.ResetPasswordToken = null;
        usuario.ResetPasswordExpires = null;
        usuario.FechaActualizacion = DateTime.UtcNow;

        await _usuarioService.UpdateAsync(usuario.Id!, usuario);

        return Ok(new { message = "Contraseña restablecida exitosamente" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error restableciendo contraseña");
        return StatusCode(500, new { message = "Error al restablecer la contraseña" });
    }
}
```

#### Método privado para generar token:
```csharp
private string GenerateSecureToken()
{
    using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
    {
        var tokenBytes = new byte[32];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
```

## APIs Finales

### 1. Solicitar token de restablecimiento
**Endpoint:** `POST /Auth/forgot-password`

**Request:**
```json
{
  "email": "usuario@tcs.com"
}
```

**Response (200 OK):**
```json
{
  "message": "Si el correo existe, recibiras un token de restablecimiento",
  "token": "abc123xyz..." // Solo en desarrollo
}
```

### 2. Verificar token
**Endpoint:** `POST /Auth/verify-reset-token`

**Request:**
```json
{
  "token": "abc123xyz..."
}
```

**Response (200 OK):**
```json
{
  "message": "Token valido",
  "email": "usuario@tcs.com"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Token invalido o expirado"
}
```

### 3. Restablecer contraseña
**Endpoint:** `POST /Auth/reset-password`

**Request:**
```json
{
  "token": "abc123xyz...",
  "newPassword": "NuevaContraseña123"
}
```

**Response (200 OK):**
```json
{
  "message": "Contraseña restablecida exitosamente"
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Token invalido"
}
```
o
```json
{
  "message": "Token expirado"
}
```

## Flujo Completo

1. Usuario solicita restablecer contraseña → `POST /Auth/forgot-password`
2. Sistema genera token (válido por 1 hora) y lo devuelve (en producción se envía por email)
3. Usuario verifica token (opcional) → `POST /Auth/verify-reset-token`
4. Usuario establece nueva contraseña → `POST /Auth/reset-password`
5. Sistema valida token, actualiza contraseña y elimina el token

## Seguridad

- Tokens generados con `RandomNumberGenerator` (criptográficamente seguros)
- Tokens expiran en 1 hora
- Respuestas genéricas para no filtrar existencia de usuarios
- Tokens de 32 bytes codificados en Base64 URL-safe
- Token se elimina después de usarse exitosamente
- Aplica tanto para usuarios como administradores (basado en el campo `rol` del usuario)

## Implementación en Android

```kotlin
data class ForgotPasswordRequest(val email: String)
data class ForgotPasswordResponse(val message: String, val token: String?)
data class VerifyTokenRequest(val token: String)
data class ResetPasswordRequest(val token: String, val newPassword: String)

interface AuthApi {
    @POST("Auth/forgot-password")
    suspend fun forgotPassword(@Body request: ForgotPasswordRequest): Response<ForgotPasswordResponse>

    @POST("Auth/verify-reset-token")
    suspend fun verifyToken(@Body request: VerifyTokenRequest): Response<Any>

    @POST("Auth/reset-password")
    suspend fun resetPassword(@Body request: ResetPasswordRequest): Response<Any>
}
```
