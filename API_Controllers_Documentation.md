# Documentación API — Controllers (ASP.NET Core / MongoDB)

Índice
- ActividadController.cs
- ConfiguracionController.cs
- ConversacionController.cs
- DocumentoController.cs
- FAQController.cs
- MensajeAutomaticoController.cs
- UsuarioController.cs
- WeatherForecastController.cs

---

## ActividadController.cs

### 1. Nombre del Controller
- Archivo: ActividadController.cs  
- Propósito: Gestionar las operaciones CRUD y consultas específicas para actividades de onboarding (crear, leer, actualizar, eliminar y filtros por día/tipo/obligatoriedad).

### 2. Entidad / Colección relacionada
- Modelo: Actividad
- Colección MongoDB (probable): `actividades`
- Campos importantes:
  - id (string, ObjectId)
  - titulo (string) — requerido
  - descripcion (string) — requerido
  - dia (int)
  - duracionHoras (double)
  - horaInicio (string, "HH:mm")
  - horaFin (string, "HH:mm")
  - lugar (string)
  - modalidad (string)
  - tipo (string)
  - categoria (string)
  - responsable (string)
  - capacidadMaxima (int)
  - obligatorio (bool)
  - materialesNecesarios (List<string>)
  - actividadesSiguientes (List<string>)
  - estado (string)
  - fechaCreacion, fechaActualizacion (DateTime)
- Relación:
  - Referencias por id a otras actividades (actividadesSiguientes). Pueden existir referencias lógicas a usuarios o documentos por id en listas.

### 3. Endpoints detallados

- GET /api/Actividad  
  - Descripción: Devuelve todas las actividades.  
  - Parámetros: ninguno.  
  - Validaciones: ninguna.  
  - Respuestas:
    - 200 OK: Array de actividades.
    - 500: { message, error }.
  - Ejemplo response (200):
```json
[
  {
    "id": "64f8a1...",
    "titulo": "Inducción general",
    "descripcion": "Bienvenida y presentación",
    "dia": 1,
    "obligatorio": true,
    "fechaCreacion": "2025-01-10T08:00:00Z"
      }
    ]
```

- GET /api/Actividad/{id}  
  - Descripción: Recupera una actividad por id.  
  - Parámetros: route id (string).  
  - Validaciones: retorna 404 si no existe.  
  - Respuestas:
    - 200 OK: Actividad.
    - 404 Not Found: { message }.
    - 500 Internal Server Error.

- POST /api/Actividad  
  - Descripción: Crea una nueva actividad.  
  - Body (explicado): Objeto con campos de Actividad; `titulo` y `descripcion` obligatorios. Otros campos opcionales.  
  - Validaciones:
    - ModelState.IsValid.
    - titulo no vacío.
    - descripcion no vacía.
  - Respuestas:
    - 201 Created: entidad creada (con Location header).
    - 400 Bad Request: ModelState o campos faltantes.
    - 500 Internal Server Error.
  - Ejemplo request:
```json
{
      "titulo": "Taller seguridad",
      "descripcion": "Capacitación de seguridad",
      "dia": 3,
      "horaInicio": "10:00",
      "horaFin": "12:00",
      "obligatorio": true
    }
```
  - Ejemplo response (201):
```json
{
      "id": "6501b2...",
      "titulo": "Taller seguridad",
      "descripcion": "Capacitación de seguridad",
      "dia": 3
    }
```

- PUT /api/Actividad/{id}  
  - Descripción: Actualiza una actividad existente.  
  - Body: Objeto Actividad con los campos a actualizar; `titulo` y `descripcion` requeridos según validación del controller.  
  - Validaciones:
    - ModelState.IsValid.
    - titulo y descripcion no vacíos.
    - Si id no existe -> 404.
  - Respuestas:
    - 204 No Content (actualizado).
    - 400 Bad Request.
    - 404 Not Found.
    - 500 Internal Server Error.

- DELETE /api/Actividad/{id}  
  - Descripción: Elimina una actividad por id.  
  - Respuestas:
    - 204 No Content.
    - 404 Not Found.
    - 500 Internal Server Error.

- GET /api/Actividad/dia/{dia}  
  - Descripción: Devuelve actividades cuyo campo `dia` coincide con el parámetro.  
  - Parámetros: dia (int).  
  - Respuestas: 200 lista, 500 error.

- GET /api/Actividad/tipo/{tipo}  
  - Descripción: Filtra actividades por `tipo`.  
  - Parámetros: tipo (string).  
  - Respuestas: 200 lista, 500 error.

- GET /api/Actividad/obligatorias  
  - Descripción: Devuelve actividades con `obligatorio = true`.  
  - Respuestas: 200 lista, 500 error.

### 4. Lógica de negocio
- Uso de ActividadService que encapsula interacción con MongoDB (IMongoCollection<Actividad>).
- Consultas comunes:
  - GetAll: Find(FilterDefinition.Empty).ToListAsync()
  - GetById: Find(filter by _id).FirstOrDefaultAsync()
  - Create: InsertOneAsync
  - Update: ReplaceOneAsync o UpdateOne
  - Delete: DeleteOneAsync
  - Filtros por `dia`, `tipo`, `obligatorio` con Builders<T>.Filter.Eq
- Manejo de errores:
  - try/catch en controller, registro con ILogger y respuesta 500 con { message, error }.
  - Validaciones de entrada devuelven 400 con detalles.

### 5. Guía para consumo desde Android (Kotlin + Retrofit)
- Llamadas:
  - GET y POST/PUT/DELETE a las rutas indicadas.
  - Para POST/PUT usar header `Content-Type: application/json`.
- Modelos Kotlin recomendados:
  - data class Actividad(val id: String?, val titulo: String, val descripcion: String, val dia: Int?, val duracionHoras: Double?, val horaInicio: String?, val horaFin: String?, val lugar: String?, val modalidad: String?, val tipo: String?, val categoria: String?, val responsable: String?, val capacidadMaxima: Int?, val obligatorio: Boolean?, val materialesNecesarios: List<String>?, val actividadesSiguientes: List<String>?, val estado: String?, val fechaCreacion: String?)
- Tipos de respuesta:
  - GET /api/Actividad -> List<Actividad>
  - GET /api/Actividad/{id} -> Actividad
  - POST -> Actividad (201)
  - PUT/DELETE -> 204 sin body
- Autenticación:
  - En caso de JWT, enviar `Authorization: Bearer <token>` en cada llamada protegida.
- Recomendaciones:
  - Usar coroutines (suspend) y mapper que transforme JSON a modelos de dominio.
  - Manejar códigos 4xx/5xx en repositorio.

### 6. Requisitos previos
- Indices recomendados: campo `dia`, `tipo`, `obligatorio`.
- Validar formato de fechas/horas (ISO / "HH:mm") y estandarizar en UTC.
- Seguridad: proteger endpoints de creación/edición/eliminación mediante roles.

---

## ConfiguracionController.cs

### 1. Nombre del Controller
- Archivo: ConfiguracionController.cs  
- Propósito: Gestionar configuraciones del sistema (CRUD) y consultas por tipo/nombre/activas.

### 2. Entidad / Colección relacionada
- Modelo: Configuracion
- Colección MongoDB (probable): `configuraciones`
- Campos importantes:
  - id (string)
  - tipo (string) — requerido
  - nombre (string) — requerido
  - descripcion (string)
  - configuracion (documento dinámico / BsonDocument)
  - activo (bool)
  - fechaCreacion, fechaActualizacion
  - modificadoPor (string)
- Relación:
  - `configuracion` es un documento flexible que puede referenciar otros recursos o contener parámetros heterogéneos.

### 3. Endpoints detallados

- GET /api/Configuracion  
  - Descripción: Lista todas las configuraciones.  
  - Respuestas: 200 lista, 500 error.

- GET /api/Configuracion/{id}  
  - Descripción: Obtiene una configuración por id.  
  - Respuestas: 200 objeto, 404, 500.

- POST /api/Configuracion  
  - Descripción: Crea una nueva configuración.  
  - Body: objeto con `tipo`, `nombre`, `descripcion`, `configuracion` (objeto JSON), `activo`.  
  - Validaciones:
    - ModelState.IsValid.
    - nombre y tipo no vacíos.
  - Respuestas:
    - 201 Created con entidad.
    - 400 Bad Request.
    - 500 Internal Server Error.
  - Ejemplo request:
```json
{
      "tipo": "chatbot",
      "nombre": "config_inicial",
      "descripcion": "Parámetros iniciales",
      "configuracion": { "maxTokens": 1000, "model": "gpt-x" },
      "activo": true
    }
```

- PUT /api/Configuracion/{id}  
  - Descripción: Actualiza configuración.  
  - Validaciones: ModelState y campos requeridos. 404 si no existe.  
  - Respuestas: 204, 400, 404, 500.

- DELETE /api/Configuracion/{id}  
  - Descripción: Elimina una configuración.  
  - Respuestas: 204, 404, 500.

- GET /api/Configuracion/tipo/{tipo}  
  - Descripción: Filtra por `tipo`.  
  - Respuestas: 200, 500.

- GET /api/Configuracion/activas  
  - Descripción: Devuelve configuraciones con `activo = true`.  
  - Respuestas: 200, 500.

- GET /api/Configuracion/nombre/{nombre}  
  - Descripción: Busca configuración por nombre exacto.  
  - Respuestas: 200 objeto o 404, 500.

### 4. Lógica de negocio
- ConfiguracionService encapsula acceso a MongoDB.
- El campo `configuracion` (BsonDocument) permite estructura dinámica; búsquedas en campos internos requieren filtros anidados o índices específicos.
- Manejo de errores con try/catch y logging.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin sugerido:
  - data class Configuracion(val id: String?, val tipo: String, val nombre: String, val descripcion: String?, val configuracion: Map<String, Any>?, val activo: Boolean)
- Consumo:
  - GET -> List<Configuracion>
  - POST/PUT -> enviar JSON con `configuracion` como Map<String, Any> o JsonObject
- Headers:
  - Authorization si aplica; `Content-Type: application/json`.
- Recomendación: No almacenar configuraciones sensibles en cliente sin cifrado.

### 6. Requisitos previos
- Control de acceso para modificar configuraciones sensibles.
- Dependencias: MongoDB.Driver; serializador JSON robusto para BsonDocument.
- Indices por `tipo`, `nombre`, `activo` según uso.

---

## ConversacionController.cs

### 1. Nombre del Controller
- Archivo: ConversacionController.cs  
- Propósito: Gestión de conversaciones del chatbot: CRUD, agregar mensajes, filtros por usuario/estado/resuelto.

### 2. Entidad / Colección relacionada
- Modelo: Conversacion
- Colección MongoDB (probable): `conversaciones`
- Campos importantes:
  - id (string)
  - usuarioId (string) — referencia a Usuario
  - mensajes (List<Mensaje>) — subdocumentos
  - fechaInicio (DateTime)
  - fechaUltimoMensaje (DateTime)
  - activa (bool)
  - resuelto (bool)
  - satisfaccion (int?)
- Relaciones:
  - usuarioId referencia lógica a la colección `usuarios`.
  - mensajes pueden referenciar `FAQ` u otros recursos por id.

### 3. Endpoints detallados

- GET /api/Conversacion  
  - Descripción: Lista todas las conversaciones.  
  - Respuestas: 200 lista, 500.

- GET /api/Conversacion/{id}  
  - Descripción: Devuelve conversación por id.  
  - Respuestas: 200, 404, 500.

- POST /api/Conversacion  
  - Descripción: Crea conversación nueva.  
  - Body: Conversacion parcial; `usuarioId` obligatorio.  
  - Validaciones: ModelState.IsValid, usuarioId no vacío.  
  - Respuestas: 201 Created, 400, 500.
  - Ejemplo request:
```json
{
      "usuarioId": "64f2e1...",
      "mensajes": [
        { "tipo": "usuario", "contenido": "Hola" }
      ]
    }
```

- PUT /api/Conversacion/{id}  
  - Descripción: Actualiza conversación.  
  - Validaciones: ModelState, usuarioId no vacío, 404 si no existe.  
  - Respuestas: 204, 400, 404, 500.

- DELETE /api/Conversacion/{id}  
  - Descripción: Elimina conversación.  
  - Respuestas: 204, 404, 500.

- GET /api/Conversacion/usuario/{usuarioId}  
  - Descripción: Obtiene conversaciones de un usuario.  
  - Respuestas: 200 lista, 500.

- GET /api/Conversacion/activas  
  - Descripción: Conversaciones activas.  
  - Respuestas: 200 lista, 500.

- GET /api/Conversacion/resueltas  
  - Descripción: Conversaciones resueltas.  
  - Respuestas: 200 lista, 500.

- POST /api/Conversacion/{id}/mensajes  
  - Descripción: Agrega un Mensaje al arreglo `mensajes` de la conversación.  
  - Body: Mensaje; campo `contenido` obligatorio.  
  - Validaciones:
    - ModelState.IsValid.
    - contenido no vacío.
    - Si no existe conversation id -> 404.
  - Respuestas:
    - 200 OK: { message: "Mensaje agregado exitosamente" }.
    - 400 Bad Request.
    - 404 Not Found.
    - 500 Internal Server Error.
  - Ejemplo request:
```json
{
      "tipo": "bot",
      "contenido": "Hola, ¿en qué puedo ayudarte?",
      "timestamp": "2025-11-21T10:05:00Z"
    }
```

### 4. Lógica de negocio
- ConversacionService realiza:
  - CRUD con InsertOneAsync / Find / ReplaceOneAsync / DeleteOneAsync.
  - AddMensajeAsync usa UpdateOne con $push para agregar subdocumentos de mensaje y actualiza fechaUltimoMensaje; puede actualizar estado `resuelto` o métricas.
- Los mensajes son subdocumentos (embedded documents) en la colección.
- Manejo de errores con logging y respuesta estandarizada 500.

### 5. Guía Android (Kotlin + Retrofit)
- Modelos Kotlin:
  - data class Mensaje(val tipo: String, val contenido: String, val timestamp: String?, val faqRelacionada: String?)
  - data class Conversacion(val id: String?, val usuarioId: String, val mensajes: List<Mensaje>?, val fechaInicio: String?, val activa: Boolean?, val resuelto: Boolean?)
- Consumo:
  - POST /api/Conversacion -> crea conversación.
  - POST /api/Conversacion/{id}/mensajes -> agrega Mensaje y espera 200 con confirmación.
  - GET endpoints devuelven Conversacion o List<Conversacion>.
- Autenticación: Authorization Bearer si aplica.
- Recomendación: Para mensajería en tiempo real usar WebSockets; si no, usar polling corto o push notifications.

### 6. Requisitos previos
- Indices recomendados: `usuarioId`, `activa`, `resuelto`.
- Validar integridad entre `usuarioId` y usuarios.
- Manejar idempotencia al agregar mensajes (evitar duplicados).

---

## DocumentoController.cs

### 1. Nombre del Controller
- Archivo: DocumentoController.cs  
- Propósito: CRUD y consultas para documentos (manuales, guías, formularios) gestionados por la plataforma.

### 2. Entidad / Colección relacionada
- Modelo: Documento
- Colección MongoDB (probable): `documentos`
- Campos importantes:
  - id (string)
  - titulo (string) — requerido
  - descripcion (string)
  - url (string) — requerido
  - tipo (string)
  - categoria (string)
  - tags (List<string>)
  - obligatorio (bool)
  - idioma, version, autor, fechaPublicacion
  - descargas (int)
- Relación:
  - Documentos pueden ser referenciados desde FAQs o actividades.

### 3. Endpoints detallados

- GET /api/Documento  
  - Lista documentos. 200 / 500.

- GET /api/Documento/{id}  
  - Devuelve documento por id. 200 / 404 / 500.

- POST /api/Documento  
  - Crea documento; validaciones: titulo y url obligatorios.  
  - Respuestas: 201 Created, 400, 500.  
  - Ejemplo request:
```json
{
  "titulo": "Manual de Uso",
  "descripcion": "Manual para empleados",
  "url": "https://cdn.ejemplo.com/manual.pdf",
  "tipo": "PDF",
  "categoria": "Manuales",
  "tags": ["inicio", "manual"],
      "obligatorio": true
    }
```

- PUT /api/Documento/{id}  
  - Actualiza documento; validaciones: titulo y url requeridos; 404 si no existe.  
  - Respuestas: 204, 400, 404, 500.

- DELETE /api/Documento/{id}  
  - Elimina documento. 204, 404, 500.

- GET /api/Documento/categoria/{categoria}  
  - Filtra por categoria. 200, 500.

- GET /api/Documento/tipo/{tipo}  
  - Filtra por tipo. 200, 500.

- GET /api/Documento/tag/{tag}  
  - Busca documentos cuyo array `tags` contenga el tag. 200, 500.

### 4. Lógica de negocio
- DocumentoService maneja:
  - InsertOneAsync, Find, ReplaceOneAsync, DeleteOneAsync.
  - Search by tags usando filtro $in o Builders<T>.Filter.AnyEq.
- Manejo de errores con try/catch y logging.
- Se recomienda validar que `url` tenga formato válido y, si aplica, accesibilidad.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin:
  - data class Documento(val id: String?, val titulo: String, val descripcion: String?, val url: String, val tipo: String?, val categoria: String?, val tags: List<String>?, val obligatorio: Boolean?)
- Consumo:
  - GET -> List<Documento> / Documento.
  - POST/PUT -> enviar JSON; recibir 201 o 204.
  - GET /tag/{tag} -> List<Documento>.
- Despliegue de archivos:
  - Para descargar abrir el campo `url` en navegador o WebView; no necesariamente pasar por API.
- Headers: Authorization si corresponde.

### 6. Requisitos previos
- Indices sobre `tags`, `categoria`, `tipo`.
- Para métricas, usar UpdateOne con $inc para conteos atomizados.
- Control de permisos para recursos privados.

---

## FAQController.cs

### 1. Nombre del Controller
- Archivo: FAQController.cs  
- Propósito: CRUD y búsqueda de FAQs almacenadas en MongoDB.

### 2. Entidad / Colección relacionada
- Modelo: FAQ
- Colección MongoDB (probable): `faqs`
- Campos importantes:
  - id (string)
  - pregunta (string) — requerido
  - respuesta (string) — requerido
  - categoria (string) — requerido
  - palabrasClave (List<string>)
  - activa (bool)
  - vecesUsada (int)
  - rating (double)
  - documentosRelacionados (List<string>)
  - fechaCreacion, fechaActualizacion
- Relación:
  - documentosRelacionados por id a la colección `documentos`.
  - actividadesRelacionadas por id a `actividades`.

### 3. Endpoints detallados

- GET /api/FAQ  
  - Lista todas las FAQs. 200, 500.

- GET /api/FAQ/{id}  
  - Obtiene FAQ por id. 200, 404, 500.

- POST /api/FAQ  
  - Crea FAQ; validaciones: pregunta, respuesta, categoria obligatorios.  
  - Respuestas: 201, 400, 500.
  - Ejemplo request:
```json
{
      "pregunta": "¿Cómo me registro?",
      "respuesta": "Ingresa a la aplicación y presiona Registro",
      "categoria": "Cuenta",
      "palabrasClave": ["registro","cuenta"]
    }
```

- PUT /api/FAQ/{id}  
  - Actualiza FAQ; validations similares a POST; 204/400/404/500.

- DELETE /api/FAQ/{id}  
  - Elimina FAQ. 204/404/500.

- GET /api/FAQ/search?query={query}  
  - Busca texto en `pregunta`, `respuesta` y `palabrasClave`.  
  - Parámetro: query (string).  
  - Respuestas: 200 lista, 500.
  - Ejemplo: GET /api/FAQ/search?query=registro

### 4. Lógica de negocio
- MongoDBService implementa:
  - CRUD: InsertOneAsync, Find, ReplaceOneAsync, DeleteOneAsync.
  - SearchFAQsAsync: puede usar filtros regex sobre campos `pregunta` y `respuesta` o un índice de texto para eficiencia.
- Manejo de errores y logging consistente.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin:
  - data class FAQ(val id: String?, val pregunta: String, val respuesta: String, val categoria: String, val palabrasClave: List<String>?)
- Consumo:
  - GET /api/FAQ -> List<FAQ>
  - GET /api/FAQ/{id} -> FAQ
  - POST /api/FAQ -> crear FAQ
  - GET /api/FAQ/search?query=xxx -> List<FAQ>
- Recomendación: Paginación en búsquedas; destacar coincidencias en UI.

### 6. Requisitos previos
- Indices: `pregunta`, `categoria`; índice de texto opcional en `pregunta` y `respuesta` para SearchFAQsAsync.
- Control de permisos para creación/edición/eliminación.

---

## MensajeAutomaticoController.cs

### 1. Nombre del Controller
- Archivo: MensajeAutomaticoController.cs  
- Propósito: CRUD y consultas para mensajes automáticos del sistema (bienvenida, recordatorios, notificaciones).

### 2. Entidad / Colección relacionada
- Modelo: MensajeAutomatico
- Colección MongoDB (probable): `mensajesAutomaticos`
- Campos importantes:
  - id (string)
  - titulo (string) — requerido
  - contenido (string) — requerido
  - tipo (string)
  - diaGatillo (int?)
  - horaEnvio (string "HH:mm")
  - canal (List<string>)
  - prioridad (string)
  - activo (bool)
  - segmento (string)
  - condicion (string)
  - fechaCreacion, creadoPor
- Relación:
  - Segmentos o condiciones pueden referenciar atributos de Usuarios.

### 3. Endpoints detallados

- GET /api/MensajeAutomatico  
  - Lista mensajes automáticos. 200, 500.

- GET /api/MensajeAutomatico/{id}  
  - Devuelve mensaje por id. 200, 404, 500.

- POST /api/MensajeAutomatico  
  - Crea mensaje; validaciones: titulo y contenido obligatorios.  
  - Respuestas: 201, 400, 500.
  - Ejemplo request:
```json
{
      "titulo": "Bienvenida",
      "contenido": "Bienvenido a la empresa",
      "tipo": "bienvenida",
      "canal": ["chatbot"],
      "activo": true,
      "horaEnvio": "09:00"
    }
```

- PUT /api/MensajeAutomatico/{id}  
  - Actualiza mensaje; 204/400/404/500.

- DELETE /api/MensajeAutomatico/{id}  
  - Elimina mensaje. 204/404/500.

- GET /api/MensajeAutomatico/tipo/{tipo}  
  - Filtra por `tipo`. 200, 500.

- GET /api/MensajeAutomatico/activos  
  - Devuelve mensajes con `activo = true`. 200, 500.

### 4. Lógica de negocio
- MensajeAutomaticoService maneja CRUD y filtros.
- Para envío programado se espera integración con scheduler (consulta de mensajes activos y aplicación de reglas de `diaGatillo`, `horaEnvio`, `segmento`).
- Validación de `horaEnvio` y formato en servicio.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin:
  - data class MensajeAutomatico(val id: String?, val titulo: String, val contenido: String, val tipo: String?, val diaGatillo: Int?, val prioridad: String?, val canal: List<String>?, val activo: Boolean?, val segmento: String?, val horaEnvio: String?)
- Consumo:
  - GET /api/MensajeAutomatico -> List<MensajeAutomatico>
  - GET /api/MensajeAutomatico/{id} -> MensajeAutomatico
  - POST/PUT -> crear/actualizar
- Headers: Authorization si aplica.
- Nota: App puede filtrar por `segmento` para presentar mensajes relevantes al usuario.

### 6. Requisitos previos
- Índices sobre `activo`, `tipo`, `diaGatillo`.
- Control de acceso para gestión de mensajes que se envían a usuarios.

---

## UsuarioController.cs

### 1. Nombre del Controller
- Archivo: UsuarioController.cs  
- Propósito: CRUD y consultas específicas sobre usuarios (búsqueda por email, DNI, departamento, estado de onboarding, activos).

### 2. Entidad / Colección relacionada
- Modelo: Usuario
- Colección MongoDB (probable): `usuarios`
- Campos importantes:
  - id (string)
  - nombre (string) — requerido
  - apellidos (string)
  - email (string) — requerido
  - telefono (string)
  - dni (string)
  - departamento (string)
  - puesto (string)
  - estadoOnboarding (string)
  - progresoOnboarding (int)
  - actividadesCompletadas (List<string>)
  - documentosEntregados (List<string>)
  - activo (bool)
  - primerLogin, ultimoLogin (DateTime?)
  - fechaCreacion, fechaActualizacion
- Relación:
  - Puede referenciar actividades/documentos por id en listas.

### 3. Endpoints detallados

- GET /api/Usuario  
  - Devuelve todos los usuarios. 200, 500.

- GET /api/Usuario/{id}  
  - Devuelve usuario por id. 200, 404, 500.

- POST /api/Usuario  
  - Crea usuario; validaciones: `email` y `nombre` obligatorios.  
  - Respuestas: 201 Created, 400, 500.
  - Ejemplo request:
```json
{
      "nombre": "Juan",
      "apellidos": "Pérez",
      "email": "juan.perez@ejemplo.com",
      "dni": "12345678"
    }
```

- PUT /api/Usuario/{id}  
  - Actualiza usuario; validaciones: email y nombre. 204/400/404/500.

- DELETE /api/Usuario/{id}  
  - Elimina usuario. 204/404/500.

- GET /api/Usuario/email/{email}  
  - Busca usuario por email. 200, 404, 500. (Recordar url-encode del email)

- GET /api/Usuario/dni/{dni}  
  - Busca usuario por DNI. 200, 404, 500.

- GET /api/Usuario/onboarding/{estado}  
  - Filtra por estado de onboarding. 200, 500.

- GET /api/Usuario/activos  
  - Devuelve usuarios con `activo = true`. 200, 500.

- GET /api/Usuario/departamento/{departamento}  
  - Filtra por departamento. 200, 500.

### 4. Lógica de negocio
- UsuarioService realiza CRUD y búsquedas:
  - GetByEmailAsync y GetByDniAsync con filtros exactos.
  - GetByEstadoOnboardingAsync con filtro por `estadoOnboarding`.
  - GetActivosAsync con `activo = true`.
- En creación/actualización se puede normalizar email y calcular campos derivados.
- Errores se logran con logger y se retornan como 500 con { message, error }.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin:
  - data class Usuario(val id: String?, val nombre: String, val apellidos: String?, val email: String, val telefono: String?, val dni: String?, val departamento: String?, val activo: Boolean?)
- Consumo:
  - GET /api/Usuario -> List<Usuario>
  - GET /api/Usuario/{id} -> Usuario
  - POST /api/Usuario -> crear usuario (recibir 201)
  - GET /api/Usuario/email/{email} -> Usuario
- Headers: Authorization si aplica.
- Notas: Al pasar email como parte de la ruta, aplicar url-encoding.

### 6. Requisitos previos
- Indices únicos recomendados: `email`, `dni` (si se requiere unicidad).
- Control de acceso para endpoints de modificación.
- Validación de datos en cliente y servidor (email regex, tamaño de campos).

---

## WeatherForecastController.cs

### 1. Nombre del Controller
- Archivo: WeatherForecastController.cs  
- Propósito: Endpoint de ejemplo que genera pronósticos del tiempo; útil para pruebas y diagnóstico de la API.

### 2. Entidad / Colección relacionada
- Modelo: WeatherForecast (temporal, no persistente)
- Colección MongoDB: No aplica
- Campos importantes:
  - date (DateOnly / ISO date)
  - temperatureC (int)
  - temperatureF (int, calculable)
  - summary (string)
- Relación: ninguna

### 3. Endpoints detallados

- GET /WeatherForecast  
  - Descripción: Genera lista de pronósticos aleatorios (5 elementos).  
  - Parámetros: ninguno.  
  - Respuestas:
    - 200 OK: Array de pronósticos.
  - Ejemplo response:
```json
[
      { "date": "2025-11-22", "temperatureC": 18, "temperatureF": 64, "summary": "Mild" }
    ]
```

### 4. Lógica de negocio
- Genera datos dinámicamente en memoria con Random.Shared; no consulta base de datos ni servicios externos.

### 5. Guía Android (Kotlin + Retrofit)
- Modelo Kotlin:
  - data class WeatherForecast(val date: String, val temperatureC: Int, val temperatureF: Int, val summary: String?)
- Consumo:
  - GET /WeatherForecast -> List<WeatherForecast>
- Uso: pruebas de conectividad / sanity check.

### 6. Requisitos previos
- Ninguno específico; endpoint público si la configuración lo permite.

---

# Requisitos generales y buenas prácticas para todos los Controllers

1. Seguridad / Autenticación
- Aunque los controllers no muestran atributos [Authorize], en producción se recomienda proteger endpoints que crean/actualizan/eliminan con JWT y roles (admin, manager).
- Header esperado (si aplica): Authorization: Bearer <token>

2. Dependencias servidor
- MongoDB.Driver (IMongoCollection<T>, Builders<T>).
- ILogger<T> en controllers para logging.
- IOptions<MongoDBSettings> para configuración de conexión.

3. JSON y tipos
- Las entidades usan ObjectId en Mongo; en API se exponen como string. En Android mapear como String.
- Fechas: usar ISO8601 (UTC) para interoperabilidad; mapear a java.time.Instant/LocalDate/LocalDateTime según el caso.

4. Manejo de errores
- Controllers usan try/catch, registran error y devuelven StatusCode(500) con JSON { message, error }.
- Validaciones devuelven 400 con ModelState o mensajes personalizados en body.

5. Rendimiento y escalabilidad
- Crear índices en MongoDB para campos usados en filtros frecuentes (email, dni, usuarioId, activo, tipo, tags, categoria).
- Para colecciones grandes implementar paginación (limit/skip o paginación basada en cursor).

6. Consumo desde Android — recomendaciones generales
- Usar Retrofit + Moshi/Gson + coroutines (suspend) para llamadas asíncronas.
- Implementar una capa Repository que traduzca DTOs de red a modelos de dominio.
- Manejar respuestas 204 (sin body) adecuadamente en Retrofit (usar Unit/Response<Void>).
- Implementar manejo centralizado de errores (interceptor en Retrofit) para mapear códigos 401/403/429/5xx.
- Implementar reintentos backoff para llamadas no idempotentes con cuidado.

---

Si quieres, puedo generar adicionalmente:
- Modelos Kotlin (data classes) más completos para cada entidad.
- Un resumen tabular de todas las rutas en formato Markdown o CSV.
- Ejemplos de payloads de error (400/404/500) estandarizados.

Indica si quieres que genere alguno de esos recursos adicionales.
