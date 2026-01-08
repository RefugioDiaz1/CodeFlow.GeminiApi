using CodeFlow.GeminiApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CodeFlow.GeminiApi.Controllers
{
    [ApiController]
    [Route("api/whatauto")]
    public class WhatautoController : ControllerBase
    {
        private readonly IGenAiService _gen;
        private readonly ILogger<WhatautoController> _logger;

        // 🧠 MEMORIA SIMPLE POR TELÉFONO (RAM)
        private static readonly Dictionary<string, List<string>> _memory = new();

        public WhatautoController(
            IGenAiService gen,
            ILogger<WhatautoController> logger)
        {
            _gen = gen;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // 🔴 Leer BODY CRUDO
            Request.EnableBuffering();

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation("Whatauto RAW BODY: {body}", rawBody);

            if (string.IsNullOrWhiteSpace(rawBody))
                return Ok(new { reply = "No recibí ningún mensaje 😅" });

            string? message = null;
            string? phone = null;
            string? sender = null;

            // 🟢 FORM URL ENCODED (Whatauto REAL)
            // Ejemplo:
            // app=WhatsApp+Business&sender=Refugio&phone=9933836139&message=hola
            if (rawBody.Contains("="))
            {
                var pairs = rawBody.Split('&', StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in pairs)
                {
                    var kv = pair.Split('=', 2);
                    if (kv.Length != 2) continue;

                    var key = kv[0].ToLower();
                    var value = Uri.UnescapeDataString(kv[1]).Replace("+", " ");

                    switch (key)
                    {
                        case "message":
                            message = value;
                            break;

                        case "phone":
                            phone = value;
                            break;

                        case "sender":
                            sender = value;
                            break;
                    }
                }
            }


            if (string.IsNullOrWhiteSpace(message))
                return Ok(new { reply = "No pude leer tu mensaje 😕" });

            // 🧠 Inicializar memoria por teléfono
            phone ??= "anonimo";

            if (!_memory.ContainsKey(phone))
                _memory[phone] = new List<string>();

            // Guarda mensaje del usuario
            _memory[phone].Add($"Usuario ({sender ?? "Cliente"}): {message}");

            // 🔁 Construir contexto (últimos 10 mensajes)
            var contexto = string.Join(
                "\n",
                _memory[phone].TakeLast(10)
            );

            // 🧠 PROMPT FINAL PARA GEMINI
            var promptFinal = $"""
Reglas estrictas:
- NO te presentes otra vez si ya saludaste.
- NO repitas el saludo inicial.
- Mantén continuidad de la conversación.
- Responde claro, amable y profesional.

Conversación previa:
{contexto}

Mensaje actual del cliente:
{message}
""";

            // 🤖 Llamada a Gemini
            var respuestaGemini = await _gen.GenerarPlaneacionAsync(promptFinal);

            // Guarda respuesta del bot
            _memory[phone].Add($"Asistente: {respuestaGemini}");

            // 🧾 Respuesta final para Whatauto
            var respuestaFinal = $"""
*ChatBot*

{respuestaGemini}

Atención Automática
""";

            return Ok(new { reply = respuestaFinal });
        }
    }
}
