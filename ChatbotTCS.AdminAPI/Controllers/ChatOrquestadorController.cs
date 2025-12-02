using ChatbotTCS.AdminAPI.Services;
using ChatbotTCS.AdminAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ChatbotTCS.AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatOrquestadorController : ControllerBase
    {
        private readonly OllamaService2 _ollamaService;
        private readonly RagMongoService _ragService;
        private readonly ActividadService _actividadService;
        private readonly DocumentoService _documentoService;
        private readonly UsuarioService _usuarioService;
        private readonly ConversacionService _conversacionService;

        public ChatOrquestadorController(
            OllamaService2 ollamaService,
            RagMongoService ragService,
            ActividadService actividadService,
            DocumentoService documentoService,
            UsuarioService usuarioService,
            ConversacionService conversacionService)
        {
            _ollamaService = ollamaService;
            _ragService = ragService;
            _actividadService = actividadService;
            _documentoService = documentoService;
            _usuarioService = usuarioService;
            _conversacionService = conversacionService;
        }

        [HttpGet("preguntar")]
        public async Task<IActionResult> Preguntar([FromQuery] string pregunta, [FromQuery] string usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId))
                return BadRequest("Error: Necesito tu ID de usuario.");

            // 1. OBTENER USUARIO
            var usuario = await _usuarioService.GetUsuarioParaChatAsync(usuarioId);
            
            if (usuario == null) return NotFound("Usuario no encontrado.");
            string nombre = usuario.Nombre;

            // 2. GESTIÓN DE HISTORIAL (INICIO)

            var conversacion = await _conversacionService.ObtenerConversacionActivaAsync(usuarioId);

            // B. Guardar pregunta del usuario
            var msjUsuario = new Mensaje { Tipo = "usuario", Contenido = pregunta, Timestamp = DateTime.UtcNow };
            await _conversacionService.AddMensajeAsync(conversacion.Id!, msjUsuario);

            // C. Obtener historial reciente para contexto
            string historialChat = await _conversacionService.ObtenerHistorialTextoAsync(usuarioId);

            // 3. CLASIFICAR INTENCIÓN
            var intencion = await _ollamaService.ClasificarIntencionAsync(pregunta);

            // 4. PREPARAR RESPUESTA
            string respuestaFinal = "";
            string promptSistema = "";

            // Variables auxiliares
            string contextoDatos = "";

            switch (intencion)
            {
                case "SALUDO":
                    promptSistema = $"Eres TCS Assistant hablando con {nombre} HABLALE POR SU NOMBRE. Responde al saludo brevemente. Historial: {historialChat}";
                    break;

                case "ACTIVIDAD":
                    var actividades = await _actividadService.GetByUsuarioIdAsync(usuarioId);
                    if (actividades.Any())
                        contextoDatos = string.Join("\n", actividades.Select(a => $"- [{a.FechaDeActividad:dd/MM/yyyy HH:mm}] {a.Titulo}"));
                    else
                        contextoDatos = "No hay actividades programadas.";

                    promptSistema = $@"
                        Eres asistente de agenda para {nombre} hablale por su NOMBRE. Hoy es: {DateTime.Now:dd/MM/yyyy}.
                        Historial: {historialChat}
                        Actividades: {contextoDatos}
                        Instrucciones: Responde sobre la agenda. SI ya paso el dia informale que ya paso";
                    break;

                case "RECURSO":
                    var termino = pregunta.ToLower().Replace("dame", "").Replace("link", "").Replace("manual", "").Trim();
                    var docs = await _documentoService.BuscarRecursosAsync(termino);

                    if (docs.Any())
                        contextoDatos = string.Join("\n", docs.Select(d => $"- {d.Titulo}: {d.Url}"));
                    else
                        contextoDatos = "No se encontraron documentos.";

                    promptSistema = $@"
                        El usuario {nombre} pide un recurso.
                        Recursos encontrados: {contextoDatos}
                        Instrucciones: Entrega los links si existen. Si no, di que no encontraste nada.";
                    break;

                case "PERFIL":
                    contextoDatos = $"Nombre: {usuario.Nombre}, Puesto: {usuario.Puesto}, Supervisor: {usuario.Supervisor?.Nombre} ({usuario.Supervisor?.Email})";
                    promptSistema = $"Eres RRHH. Datos: {contextoDatos}. Responde la duda de perfil de {nombre}.";
                    break;

                case "CONSULTA":
                default:
                    var vector = await _ollamaService.GetEmbeddingAsync(pregunta);
                    var resultados = await _ragService.SearchAsync(vector);

                    if (resultados.Count > 0 && resultados.First().Score >= 0.80)
                        contextoDatos = string.Join("\n\n", resultados.Select(r => r.Content));
                    else
                        contextoDatos = "NO_INFO";

                    promptSistema = $@"
                        Eres TCS Assistant hablando con {nombre} hablale POR SU NOMBRE.
                        HISTORIAL: {historialChat}
                        CONTEXTO OFICIAL: {contextoDatos}
                        
                        INSTRUCCIONES:
                        1. Si el contexto es 'NO_INFO', discúlpate y di que no sabes NO DES RECOMENDACIONES.
                        2. Responde usando SOLO el contexto oficial.
                        3. NO inventes nada y ni trates de responder.
                        4. Usa el historial para entender el hilo de la conversacion.
                        5.maximo 60 palabras ";
                    break;
            }

            // 5. GENERAR RESPUESTA (

            respuestaFinal = await _ollamaService.ChatAsync(promptSistema, pregunta);

            // 6. GUARDAR RESPUESTA DEL BOT
            var msjBot = new Mensaje
            {
                Tipo = "bot",
                Contenido = respuestaFinal,
                Timestamp = DateTime.UtcNow,
                IntencionDetectada = intencion
            };
            await _conversacionService.AddMensajeAsync(conversacion.Id!, msjBot);

            // 7. DEVOLVER JSON
            return Ok(new
            {
                respuesta = respuestaFinal,
                intencion = intencion,
                fuentes = (intencion == "CONSULTA" && contextoDatos != "NO_INFO") ? "Documentos RAG" : "Base de Datos"
            });
        }
    }
}