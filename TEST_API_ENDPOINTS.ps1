# ============================================================
# SCRIPT DE PRUEBAS - API CHATBOT TCS
# ============================================================
# Este script prueba TODOS los endpoints del panel administrativo
# ============================================================

$baseUrl = "http://localhost:5288/api"

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   PRUEBAS DE API - CHATBOT TCS ADMIN PANEL" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ============================================================
# 1. VERIFICAR QUE LA API ESTÉ CORRIENDO
# ============================================================
Write-Host "►► 1. VERIFICANDO QUE LA API ESTÉ CORRIENDO..." -ForegroundColor Yellow
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/mensajeautomatico" -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✓ API está corriendo en $baseUrl" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ ERROR: La API NO está corriendo en $baseUrl" -ForegroundColor Red
    Write-Host "  Por favor ejecuta:" -ForegroundColor Yellow
    Write-Host "    cd C:\C#\ChatbotTCS.AdminAPI\ChatbotTCS.AdminAPI" -ForegroundColor White
    Write-Host "    dotnet run" -ForegroundColor White
    Write-Host ""
    exit 1
}

# ============================================================
# 2. MENSAJES AUTOMÁTICOS
# ============================================================
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "►► 2. PROBANDO MENSAJES AUTOMÁTICOS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 2.1 GET - Listar todos
Write-Host "2.1 GET /api/mensajeautomatico - Listar todos" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/mensajeautomatico" -Method GET
    Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
    Write-Host "  ✓ Mensajes encontrados: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "  ✓ Primer mensaje: $($response[0].titulo)" -ForegroundColor Green
    }
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 2.2 POST - Crear nuevo
Write-Host "2.2 POST /api/mensajeautomatico - Crear nuevo" -ForegroundColor White
$newMensaje = @{
    titulo = "Mensaje de Prueba"
    contenido = "Este es un mensaje de prueba creado por el script"
    tipo = "informativo"
    prioridad = "media"
    canal = @("chatbot")
    activo = $true
    segmento = "todos"
    horaEnvio = "09:00"
    creadoPor = "Script de Prueba"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/mensajeautomatico" -Method POST -Body $newMensaje -ContentType "application/json"
    $mensajeId = $response.id
    Write-Host "  ✓ Status: 201 Created" -ForegroundColor Green
    Write-Host "  ✓ ID creado: $mensajeId" -ForegroundColor Green
    Write-Host "  ✓ Título: $($response.titulo)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $mensajeId = $null
}
Write-Host ""

# 2.3 GET - Obtener por ID
if ($mensajeId) {
    Write-Host "2.3 GET /api/mensajeautomatico/{id} - Obtener por ID" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/mensajeautomatico/$mensajeId" -Method GET
        Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Mensaje obtenido: $($response.titulo)" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 2.4 PUT - Actualizar
    Write-Host "2.4 PUT /api/mensajeautomatico/{id} - Actualizar" -ForegroundColor White
    $updateMensaje = @{
        titulo = "Mensaje de Prueba ACTUALIZADO"
        contenido = "Contenido actualizado por el script"
        tipo = "informativo"
        prioridad = "alta"
        canal = @("chatbot", "email")
        activo = $true
        segmento = "todos"
        horaEnvio = "10:00"
        creadoPor = "Script de Prueba"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/mensajeautomatico/$mensajeId" -Method PUT -Body $updateMensaje -ContentType "application/json"
        Write-Host "  ✓ Status: 204 No Content o 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Mensaje actualizado correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 2.5 DELETE - Eliminar
    Write-Host "2.5 DELETE /api/mensajeautomatico/{id} - Eliminar" -ForegroundColor White
    try {
        Invoke-RestMethod -Uri "$baseUrl/mensajeautomatico/$mensajeId" -Method DELETE
        Write-Host "  ✓ Status: 204 No Content" -ForegroundColor Green
        Write-Host "  ✓ Mensaje eliminado correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# ============================================================
# 3. ACTIVIDADES
# ============================================================
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "►► 3. PROBANDO ACTIVIDADES" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 3.1 GET - Listar todas
Write-Host "3.1 GET /api/actividad - Listar todas" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/actividad" -Method GET
    Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
    Write-Host "  ✓ Actividades encontradas: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "  ✓ Primera actividad: $($response[0].titulo)" -ForegroundColor Green
    }
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 3.2 POST - Crear nueva
Write-Host "3.2 POST /api/actividad - Crear nueva" -ForegroundColor White
$newActividad = @{
    titulo = "Actividad de Prueba"
    descripcion = "Descripción de la actividad de prueba"
    dia = 1
    duracionHoras = 2.5
    horaInicio = "09:00"
    horaFin = "11:30"
    lugar = "Sala de Reuniones"
    modalidad = "presencial"
    tipo = "induccion"
    categoria = "General"
    responsable = "RRHH"
    capacidadMaxima = 20
    obligatorio = $true
    estado = "activo"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/actividad" -Method POST -Body $newActividad -ContentType "application/json"
    $actividadId = $response.id
    Write-Host "  ✓ Status: 201 Created" -ForegroundColor Green
    Write-Host "  ✓ ID creado: $actividadId" -ForegroundColor Green
    Write-Host "  ✓ Título: $($response.titulo)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $actividadId = $null
}
Write-Host ""

# 3.3 GET - Obtener por ID
if ($actividadId) {
    Write-Host "3.3 GET /api/actividad/{id} - Obtener por ID" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/actividad/$actividadId" -Method GET
        Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Actividad obtenida: $($response.titulo)" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 3.4 PUT - Actualizar
    Write-Host "3.4 PUT /api/actividad/{id} - Actualizar" -ForegroundColor White
    $updateActividad = @{
        titulo = "Actividad de Prueba ACTUALIZADA"
        descripcion = "Descripción actualizada"
        dia = 2
        duracionHoras = 3.0
        horaInicio = "10:00"
        horaFin = "13:00"
        lugar = "Auditorio"
        modalidad = "virtual"
        tipo = "capacitacion"
        categoria = "General"
        responsable = "Capacitación"
        capacidadMaxima = 30
        obligatorio = $true
        estado = "activo"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/actividad/$actividadId" -Method PUT -Body $updateActividad -ContentType "application/json"
        Write-Host "  ✓ Status: 204 No Content o 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Actividad actualizada correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 3.5 DELETE - Eliminar
    Write-Host "3.5 DELETE /api/actividad/{id} - Eliminar" -ForegroundColor White
    try {
        Invoke-RestMethod -Uri "$baseUrl/actividad/$actividadId" -Method DELETE
        Write-Host "  ✓ Status: 204 No Content" -ForegroundColor Green
        Write-Host "  ✓ Actividad eliminada correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# ============================================================
# 4. DOCUMENTOS/RECURSOS
# ============================================================
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "►► 4. PROBANDO DOCUMENTOS/RECURSOS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# 4.1 GET - Listar todos
Write-Host "4.1 GET /api/documento - Listar todos" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/documento" -Method GET
    Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
    Write-Host "  ✓ Documentos encontrados: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "  ✓ Primer documento: $($response[0].titulo)" -ForegroundColor Green
    }
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 4.2 POST - Crear nuevo
Write-Host "4.2 POST /api/documento - Crear nuevo" -ForegroundColor White
$newDocumento = @{
    titulo = "Documento de Prueba"
    descripcion = "Descripción del documento de prueba"
    url = "https://ejemplo.com/documento.pdf"
    tipo = "PDF"
    categoria = "Manuales"
    subcategoria = "Onboarding"
    tags = @("prueba", "test")
    icono = "📄"
    idioma = "Español"
    version = "1.0"
    publico = "Nuevos empleados"
    obligatorio = $false
    autor = "Script de Prueba"
    valoracion = 0
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/documento" -Method POST -Body $newDocumento -ContentType "application/json"
    $documentoId = $response.id
    Write-Host "  ✓ Status: 201 Created" -ForegroundColor Green
    Write-Host "  ✓ ID creado: $documentoId" -ForegroundColor Green
    Write-Host "  ✓ Título: $($response.titulo)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    $documentoId = $null
}
Write-Host ""

# 4.3 GET - Obtener por ID
if ($documentoId) {
    Write-Host "4.3 GET /api/documento/{id} - Obtener por ID" -ForegroundColor White
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/documento/$documentoId" -Method GET
        Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Documento obtenido: $($response.titulo)" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 4.4 PUT - Actualizar
    Write-Host "4.4 PUT /api/documento/{id} - Actualizar" -ForegroundColor White
    $updateDocumento = @{
        titulo = "Documento de Prueba ACTUALIZADO"
        descripcion = "Descripción actualizada"
        url = "https://ejemplo.com/documento-v2.pdf"
        tipo = "PDF"
        categoria = "Políticas"
        subcategoria = "Onboarding"
        tags = @("prueba", "test", "actualizado")
        icono = "📄"
        idioma = "Español"
        version = "2.0"
        publico = "Todos"
        obligatorio = $true
        autor = "Script de Prueba"
        valoracion = 5
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/documento/$documentoId" -Method PUT -Body $updateDocumento -ContentType "application/json"
        Write-Host "  ✓ Status: 204 No Content o 200 OK" -ForegroundColor Green
        Write-Host "  ✓ Documento actualizado correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""

    # 4.5 DELETE - Eliminar
    Write-Host "4.5 DELETE /api/documento/{id} - Eliminar" -ForegroundColor White
    try {
        Invoke-RestMethod -Uri "$baseUrl/documento/$documentoId" -Method DELETE
        Write-Host "  ✓ Status: 204 No Content" -ForegroundColor Green
        Write-Host "  ✓ Documento eliminado correctamente" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

# ============================================================
# 5. MÉTRICAS
# ============================================================
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "►► 5. PROBANDO MÉTRICAS" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

Write-Host "5.1 GET /api/metrics - Obtener métricas generales" -ForegroundColor White
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/metrics" -Method GET
    Write-Host "  ✓ Status: 200 OK" -ForegroundColor Green
    Write-Host "  ✓ Total Contenidos: $($response.totalContents)" -ForegroundColor Green
    Write-Host "  ✓ Total Actividades: $($response.totalActivities)" -ForegroundColor Green
    Write-Host "  ✓ Total Recursos: $($response.totalResources)" -ForegroundColor Green
    Write-Host "  ✓ Tasa de Completitud: $($response.completionRate)%" -ForegroundColor Green
    Write-Host "  ✓ Satisfacción Promedio: $($response.averageSatisfaction)" -ForegroundColor Green
    Write-Host "  ✓ Tiempo Promedio (días): $($response.averageTimeDays)" -ForegroundColor Green
} catch {
    Write-Host "  ✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# ============================================================
# RESUMEN FINAL
# ============================================================
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "   PRUEBAS COMPLETADAS" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Si todos los tests pasaron correctamente, la API está 100% funcional" -ForegroundColor Green
Write-Host "y lista para conectarse con la aplicación Android." -ForegroundColor Green
Write-Host ""
