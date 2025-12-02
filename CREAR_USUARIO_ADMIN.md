# Crear Usuario Administrador en MongoDB

## Problema
Al intentar hacer login con las credenciales:
- **Email:** alias.rodriguez@tcs.com
- **Contraseña:** yarasa

Se obtiene un error 500, probablemente porque el usuario no existe en la base de datos MongoDB.

## Solución

### Opción 1: Usar MongoDB Compass (Recomendado)

1. **Abrir MongoDB Compass**
2. **Conectar a tu base de datos**
   - Connection String: `mongodb://localhost:27017`
3. **Seleccionar la base de datos:** `ChatbotTCS` (o el nombre que tengas configurado)
4. **Seleccionar la colección:** `usuarios`
5. **Click en "ADD DATA" → "Insert Document"**
6. **Pegar el siguiente JSON:**

```json
{
  "nombre": "Alias",
  "apellidos": "Rodriguez",
  "nombreCompleto": "Alias Rodriguez",
  "email": "alias.rodriguez@tcs.com",
  "contraseña": "yarasa",
  "telefono": "+51 999 888 777",
  "dni": "12345678",
  "fechaNacimiento": {
    "$date": "1990-01-01T00:00:00.000Z"
  },
  "edad": 34,
  "genero": "Masculino",
  "estadoCivil": "Soltero",
  "direccion": {
    "calle": "Av. Principal 123",
    "distrito": "San Isidro",
    "ciudad": "Lima",
    "pais": "Perú",
    "codigoPostal": "15046"
  },
  "area": "Tecnología",
  "departamento": "IT",
  "puesto": "Administrador de Sistemas",
  "nivel": "Senior",
  "tipoContrato": "Indefinido",
  "fechaIngreso": {
    "$date": "2024-01-01T00:00:00.000Z"
  },
  "diasDesdeIngreso": 329,
  "supervisor": {
    "nombre": "Juan Pérez",
    "email": "juan.perez@tcs.com",
    "telefono": "+51 999 777 666",
    "puesto": "Director de IT"
  },
  "estadoOnboarding": "Completado",
  "progresoOnboarding": 100,
  "actividadesCompletadas": [],
  "actividadesPendientes": [],
  "documentosEntregados": [],
  "documentosPendientes": [],
  "cursosAsignados": [],
  "cursosCompletados": [],
  "certificaciones": [],
  "favoritosChat": [],
  "preferencias": {
    "notificaciones": true,
    "notificacionesEmail": true,
    "notificacionesPush": true,
    "idioma": "es",
    "temaOscuro": false
  },
  "estadisticas": {
    "mensajesEnviados": 0,
    "preguntasRealizadas": 0,
    "documentosDescargados": 0
  },
  "activo": true,
  "verificado": true,
  "fechaCreacion": {
    "$date": "2025-11-25T00:00:00.000Z"
  },
  "fechaActualizacion": {
    "$date": "2025-11-25T00:00:00.000Z"
  },
  "creadoPor": "Sistema",
  "rol": "Administrador"
}
```

7. **Click en "Insert"**

---

### Opción 2: Usar MongoDB Shell (mongosh)

1. **Abrir terminal/PowerShell**
2. **Conectar a MongoDB:**
   ```bash
   mongosh
   ```

3. **Seleccionar la base de datos:**
   ```javascript
   use ChatbotTCS
   ```

4. **Insertar el usuario administrador:**
   ```javascript
   db.usuarios.insertOne({
     nombre: "Alias",
     apellidos: "Rodriguez",
     nombreCompleto: "Alias Rodriguez",
     email: "alias.rodriguez@tcs.com",
     contraseña: "yarasa",
     telefono: "+51 999 888 777",
     dni: "12345678",
     fechaNacimiento: new Date("1990-01-01"),
     edad: 34,
     genero: "Masculino",
     estadoCivil: "Soltero",
     direccion: {
       calle: "Av. Principal 123",
       distrito: "San Isidro",
       ciudad: "Lima",
       pais: "Perú",
       codigoPostal: "15046"
     },
     area: "Tecnología",
     departamento: "IT",
     puesto: "Administrador de Sistemas",
     nivel: "Senior",
     tipoContrato: "Indefinido",
     fechaIngreso: new Date("2024-01-01"),
     diasDesdeIngreso: 329,
     supervisor: {
       nombre: "Juan Pérez",
       email: "juan.perez@tcs.com",
       telefono: "+51 999 777 666",
       puesto: "Director de IT"
     },
     estadoOnboarding: "Completado",
     progresoOnboarding: 100,
     actividadesCompletadas: [],
     actividadesPendientes: [],
     documentosEntregados: [],
     documentosPendientes: [],
     cursosAsignados: [],
     cursosCompletados: [],
     certificaciones: [],
     favoritosChat: [],
     preferencias: {
       notificaciones: true,
       notificacionesEmail: true,
       notificacionesPush: true,
       idioma: "es",
       temaOscuro: false
     },
     estadisticas: {
       mensajesEnviados: 0,
       preguntasRealizadas: 0,
       documentosDescargados: 0
     },
     activo: true,
     verificado: true,
     fechaCreacion: new Date(),
     fechaActualizacion: new Date(),
     creadoPor: "Sistema",
     rol: "Administrador"
   })
   ```

5. **Verificar que se insertó correctamente:**
   ```javascript
   db.usuarios.findOne({ email: "alias.rodriguez@tcs.com" })
   ```

---

### Opción 3: Crear endpoint de registro en la API

Si prefieres, puedo crear un endpoint temporal para registrar administradores directamente desde la API.

---

## Verificación

Después de insertar el usuario, verifica que existe:

### Usando mongosh:
```javascript
use ChatbotTCS
db.usuarios.findOne({ email: "alias.rodriguez@tcs.com" })
```

Deberías ver el usuario con todos sus datos.

### Usando la API:
```bash
# Probar login
curl -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "alias.rodriguez@tcs.com",
    "password": "yarasa"
  }'
```

Si todo está bien, deberías recibir:
```json
{
  "message": "Login exitoso",
  "token": "eyJ...",
  "usuario": {
    "id": "...",
    "email": "alias.rodriguez@tcs.com",
    "nombreCompleto": "Alias Rodriguez",
    "rol": "Administrador",
    ...
  }
}
```

---

## Notas Importantes

1. **Seguridad:** En producción, las contraseñas deben estar hasheadas (BCrypt, Argon2, etc.)
2. **El campo `contraseña` debe coincidir exactamente** con lo que escribes en el login
3. **El campo `activo` debe ser `true`** para que el usuario pueda hacer login
4. **El campo `rol` debe ser `"Administrador"`** (con mayúscula)

---

## Solución al Error 500

El error 500 probablemente ocurre porque:

1. **El usuario no existe en la base de datos** ✅ (Solución: insertar el usuario)
2. **La colección "usuarios" no existe** (Solución: MongoDB la crea automáticamente al insertar)
3. **Problema de conexión con MongoDB** (Verificar que MongoDB esté corriendo)

---

## Comandos Útiles

### Verificar que MongoDB está corriendo:
```bash
# Windows
mongod --version

# O verifica el servicio
services.msc
# Buscar "MongoDB" y verificar que esté "Running"
```

### Ver todos los usuarios:
```javascript
use ChatbotTCS
db.usuarios.find().pretty()
```

### Eliminar usuario (si necesitas):
```javascript
db.usuarios.deleteOne({ email: "alias.rodriguez@tcs.com" })
```

### Actualizar contraseña:
```javascript
db.usuarios.updateOne(
  { email: "alias.rodriguez@tcs.com" },
  { $set: { contraseña: "nuevaContraseña" } }
)
```

---

## Siguiente Paso

Una vez insertado el usuario administrador, intenta hacer login nuevamente desde la app Android con:
- **Email:** alias.rodriguez@tcs.com
- **Contraseña:** yarasa

Debería funcionar correctamente.
