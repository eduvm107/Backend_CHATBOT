# API de Autenticación - ChatbotTCS

## Endpoints disponibles

### 1. POST /api/Auth/login
**Descripción:** Autentica un usuario con email y contraseña

**Cuerpo de la petición:**
```json
{
  "email": "jose.rodriguez@tcs.com",
  "password": "yarasa"
}
```

**Respuesta exitosa (200):**
```json
{
  "message": "Login exitoso",
  "token": null,
  "usuario": {
    "id": "69126483b0aff43d4b766b82",
    "email": "jose.rodriguez@tcs.com",
    "nombreCompleto": "José Rodriguez Pérez",
    "nombre": "José",
    "departamento": "Tecnología",
    "puesto": "Desarrollador Full Stack Junior",
    "activo": true,
    "verificado": false,
    "estadoOnboarding": "activo",
    "progresoOnboarding": 0
  }
}
```

**Respuesta de error (401):**
```json
{
  "message": "Credenciales inválidas"
}
```

### 2. GET /api/Auth/check-email/{email}
**Descripción:** Verifica si un email existe en el sistema

**Ejemplo:** `/api/Auth/check-email/jose.rodriguez@tcs.com`

**Respuesta:**
```json
{
  "exists": true,
  "active": true
}
```

### 3. POST /api/Auth/logout
**Descripción:** Cierra la sesión del usuario

**Cuerpo de la petición:**
```json
"jose.rodriguez@tcs.com"
```

**Respuesta:**
```json
{
  "message": "Logout exitoso"
}
```

## Códigos de estado HTTP

- **200 OK:** Operación exitosa
- **400 Bad Request:** Datos inválidos o faltantes
- **401 Unauthorized:** Credenciales incorrectas o usuario inactivo
- **500 Internal Server Error:** Error interno del servidor

## Ejemplo de uso con curl

```bash
# Login
curl -X POST "https://localhost:5001/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "jose.rodriguez@tcs.com",
    "password": "yarasa"
  }'

# Verificar email
curl -X GET "https://localhost:5001/api/Auth/check-email/jose.rodriguez@tcs.com"

# Logout
curl -X POST "https://localhost:5001/api/Auth/logout" \
  -H "Content-Type: application/json" \
  -d '"jose.rodriguez@tcs.com"'
```

## Notas importantes

1. **Seguridad:** Actualmente las contraseñas se almacenan en texto plano. En producción se recomienda usar hash (bcrypt, Argon2, etc.)

2. **Tokens:** El campo `token` está preparado para futuras implementaciones con JWT

3. **Login tracking:** El sistema registra `primerLogin` y `ultimoLogin` automáticamente

4. **Validaciones:** Se valida que el usuario esté activo antes de permitir el login

5. **Logs:** Todos los intentos de login se registran en los logs del sistema