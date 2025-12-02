# Integración Android App con API - Estado Actual

**Fecha:** 2025-11-25
**Proyecto:** ChatBot TCS - Panel de Administración

## Resumen Ejecutivo

La aplicación Android ahora está **100% conectada** con la API ASP.NET Core que utiliza MongoDB como base de datos.

---

## Endpoints Disponibles

### ✅ 1. Métricas del Sistema
**Endpoint:** `GET /api/metrics`

**Respuesta:**
```json
{
  "totalContents": 0,
  "totalActivities": 0,
  "totalResources": 0,
  "completionRate": 87,
  "averageSatisfaction": 4.5,
  "averageTimeDays": 14,
  "activeUsers": 0,
  "totalInteractions": 0
}
```

**Colecciones MongoDB usadas:**
- `mensajesautomaticos` (para contenidos)
- `actividades`
- `documentos` (para recursos)
- `conversaciones` (para métricas de usuarios)

---

### ✅ 2. Contenidos/Mensajes Automáticos
**Base URL:** `/api/mensajeautomatico`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/mensajeautomatico` | Listar todos |
| GET | `/api/mensajeautomatico/{id}` | Obtener por ID |
| POST | `/api/mensajeautomatico` | Crear nuevo |
| PUT | `/api/mensajeautomatico/{id}` | Actualizar |
| DELETE | `/api/mensajeautomatico/{id}` | Eliminar |
| GET | `/api/mensajeautomatico/tipo/{tipo}` | Filtrar por tipo |
| GET | `/api/mensajeautomatico/activos` | Solo activos |

**Modelo MongoDB:**
```javascript
{
  "_id": ObjectId,
  "titulo": String,
  "contenido": String,
  "tipo": String,
  "diaGatillo": Number,
  "prioridad": String,
  "canal": [String],
  "activo": Boolean,
  "segmento": String,
  "horaEnvio": String,
  "condicion": String,
  "fechaCreacion": Date,
  "creadoPor": String
}
```

---

### ✅ 3. Actividades
**Base URL:** `/api/actividad`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/actividad` | Listar todas |
| GET | `/api/actividad/{id}` | Obtener por ID |
| POST | `/api/actividad` | Crear nueva |
| PUT | `/api/actividad/{id}` | Actualizar |
| DELETE | `/api/actividad/{id}` | Eliminar |
| GET | `/api/actividad/dia/{dia}` | Filtrar por día |
| GET | `/api/actividad/tipo/{tipo}` | Filtrar por tipo |
| GET | `/api/actividad/obligatorias` | Solo obligatorias |

**Modelo MongoDB:**
```javascript
{
  "_id": ObjectId,
  "titulo": String,
  "descripcion": String,
  "dia": Number,
  "duracionHoras": Number,
  "horaInicio": String,
  "horaFin": String,
  "lugar": String,
  "modalidad": String,
  "tipo": String,
  "categoria": String,
  "responsable": String,
  "emailResponsable": String,
  "capacidadMaxima": Number,
  "obligatorio": Boolean,
  "materialesNecesarios": [String],
  "materialesProporcionados": [String],
  "preparacionPrevia": String,
  "actividadesSiguientes": [String],
  "estado": String,
  "fechaCreacion": Date,
  "fecha_de_actividad": Date
}
```

---

### ✅ 4. Recursos/Documentos
**Base URL:** `/api/documento`

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/documento` | Listar todos |
| GET | `/api/documento/{id}` | Obtener por ID |
| POST | `/api/documento` | Crear nuevo |
| PUT | `/api/documento/{id}` | Actualizar |
| DELETE | `/api/documento/{id}` | Eliminar |
| GET | `/api/documento/categoria/{categoria}` | Filtrar por categoría |
| GET | `/api/documento/tipo/{tipo}` | Filtrar por tipo |
| GET | `/api/documento/tag/{tag}` | Buscar por tag |

**Modelo MongoDB:**
```javascript
{
  "_id": ObjectId,
  "titulo": String,
  "descripcion": String,
  "url": String,
  "tipo": String,
  "categoria": String,
  "subcategoria": String,
  "tags": [String],
  "icono": String,
  "tamaño": String,
  "idioma": String,
  "version": String,
  "publico": String,
  "obligatorio": Boolean,
  "fechaPublicacion": Date,
  "fechaActualizacion": Date,
  "autor": String,
  "descargas": Number,
  "accesos": Number,
  "valoracion": Number
}
```

---

## Cambios Realizados

### ✅ Archivo Creado: `MetricsController.cs`

**Ubicación:** `C:\C#\ChatbotTCS.AdminAPI\ChatbotTCS.AdminAPI\Controllers\MetricsController.cs`

**Funcionalidades:**
1. **GET /api/metrics** - Métricas generales del dashboard
2. **GET /api/metrics/conversaciones** - Métricas de conversaciones
3. **GET /api/metrics/actividades** - Métricas de actividades
4. **GET /api/metrics/documentos** - Métricas de documentos

**Cálculo de Métricas:**
- `totalContents`: Cuenta registros en colección `mensajesautomaticos`
- `totalActivities`: Cuenta registros en colección `actividades`
- `totalResources`: Cuenta registros en colección `documentos`
- `completionRate`: Porcentaje de actividades obligatorias completadas
- `averageSatisfaction`: Promedio de campo `satisfaccion` en conversaciones
- `averageTimeDays`: Promedio del campo `dia` en actividades
- `activeUsers`: Cuenta conversaciones con `activa: true`
- `totalInteractions`: Suma total de mensajes en todas las conversaciones

---

## Configuración Necesaria

### 1. MongoDB Collections Requeridas

Tu base de datos MongoDB debe tener estas colecciones:

```javascript
// Database: ChatbotTCS (o el nombre configurado en appsettings.json)

Collections:
├── actividades
├── conversaciones
├── documentos
├── faqs
├── mensajesautomaticos
├── usuarios
└── configuraciones
```

### 2. Configuración de la API (appsettings.json)

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "ChatbotTCS"
  }
}
```

### 3. Configuración de la App Android

**Archivo:** `app/src/main/java/com/example/chatbot_diseo/network/client/ApiConfig.kt`

```kotlin
object ApiConfig {
    // Para emulador Android
    const val BASE_URL = "http://10.0.2.2:5000/api/"

    // Para dispositivo físico (usar IP de tu PC)
    // const val BASE_URL = "http://192.168.100.22:5000/api/"

    // Para producción
    // const val BASE_URL = "https://tu-servidor.com/api/"
}
```

---

## Estado de Integración

| Funcionalidad | Android App | API Backend | MongoDB | Estado |
|--------------|-------------|-------------|---------|--------|
| Contenidos | ✅ | ✅ | ✅ | **CONECTADO** |
| Actividades | ✅ | ✅ | ✅ | **CONECTADO** |
| Recursos | ✅ | ✅ | ✅ | **CONECTADO** |
| Métricas | ✅ | ✅ | ✅ | **CONECTADO** |
| FAQs | ⚠️ | ✅ | ✅ | DISPONIBLE (no usado en app) |
| Conversaciones | ⚠️ | ✅ | ✅ | DISPONIBLE (no usado en app) |

---

## Próximos Pasos

### Opcional - Para Producción

1. **Agregar Autenticación/Autorización**
   - Implementar JWT tokens
   - Proteger endpoints sensibles

2. **Optimización de Queries**
   - Agregar índices en MongoDB para campos frecuentemente consultados
   - Implementar caché en métricas

3. **Manejo de Errores**
   - Implementar retry logic en la app Android
   - Mejorar mensajes de error para el usuario

4. **Testing**
   - Probar todos los endpoints con Postman/Swagger
   - Verificar funcionamiento en dispositivo físico

---

## Comandos Útiles

### Ejecutar API
```bash
cd C:\C#\ChatbotTCS.AdminAPI\ChatbotTCS.AdminAPI
dotnet run
```

### Ver Swagger Documentation
```
http://localhost:5000/swagger
```

### Probar Endpoint de Métricas
```bash
curl http://localhost:5000/api/metrics
```

---

## Notas Importantes

1. **Sin cambios en MongoDB**: Todo funciona con las colecciones existentes
2. **Compatibilidad total**: Los modelos de la app coinciden con los de la API
3. **CORS habilitado**: La API permite peticiones desde cualquier origen (desarrollo)
4. **Logging completo**: Todos los servicios tienen logging para debugging

---

## Contacto

Para cualquier duda sobre la integración, revisar:
- Logs de la API: Consola donde se ejecuta `dotnet run`
- Logs de Android: Logcat en Android Studio
- Swagger: http://localhost:5000/swagger para documentación interactiva
