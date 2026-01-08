using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CodeFlow.GeminiApi.Services
{
    public class GenAiService : IGenAiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly ILogger<GenAiService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        public GenAiService(HttpClient http, IConfiguration cfg, ILogger<GenAiService> logger)
        {
            _http = http;
            _cfg = cfg;
            _logger = logger;

            _apiKey = _cfg["Gemini:ApiKey"];
            _model = _cfg["Gemini:ModelResource"];

            _http.Timeout = TimeSpan.FromSeconds(60);

            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("Gemini ApiKey NO cargada desde user-secrets");

            if (string.IsNullOrEmpty(_model))
                throw new Exception("Gemini ModelResource NO cargado desde user-secrets");
        }


        public async Task<string> GenerarPlaneacionAsync(string tema)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("Gemini API key no configurada (user-secrets).");

            string promptAdicional = "";

            var prompt = $"{tema}. "+ promptAdicional + "";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
     {
        new {
            parts = new[]
            {
                new { text = prompt }
            }
        }
    }
            };

            string jsonResponse = string.Empty;

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var resp = await _http.PostAsync(url, content);

                var body = await resp.Content.ReadAsStringAsync();

                // 🔴 429 → cuota excedida
                if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Gemini quota exceeded (429): {body}", body);

                    return "⚠️ En este momento el sistema está recibiendo muchas solicitudes.\nIntenta nuevamente mas tarde 🙏";
                }

                // 🔴 Otros errores de Gemini
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Error response from Gemini. Status: {status}. Body: {body}",
                        (int)resp.StatusCode,
                        body
                    );

                    return "⚠️ El servicio de IA no está disponible en este momento. Intenta más tarde.";
                }

                // 🟢 OK
                jsonResponse = body;
                _logger.LogInformation("Respuesta cruda de Gemini: {json}", jsonResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error llamando a Gemini (gemini-2.0-flash).");
                throw;
            }

            // Intento flexible de parsear rutas comunes; si no, devolvemos crudo
            // Intento flexible y robusto de parsear rutas comunes; si no, devolvemos crudo
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Función local recursiva que busca texto en el JsonElement
                string? ExtractText(JsonElement el, int depth = 0)
                {
                    if (depth > 10) return null; // evita recursion infinita

                    switch (el.ValueKind)
                    {
                        case JsonValueKind.String:
                            var s = el.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) return s;
                            return null;

                        case JsonValueKind.Object:
                            // propiedades que suelen contener texto
                            var prefer = new[] { "text", "generated_text", "message", "output", "content", "response", "result" };
                            foreach (var p in prefer)
                            {
                                if (el.TryGetProperty(p, out var prop))
                                {
                                    // si es string, devuélvelo; si es array/objeto, recurre
                                    var found = ExtractText(prop, depth + 1);
                                    if (!string.IsNullOrWhiteSpace(found)) return found;
                                }
                            }

                            // si no encontramos por nombre, recorre todas las propiedades
                            foreach (var prop in el.EnumerateObject())
                            {
                                var found = ExtractText(prop.Value, depth + 1);
                                if (!string.IsNullOrWhiteSpace(found)) return found;
                            }
                            return null;

                        case JsonValueKind.Array:
                            foreach (var item in el.EnumerateArray())
                            {
                                var found = ExtractText(item, depth + 1);
                                if (!string.IsNullOrWhiteSpace(found)) return found;
                            }
                            return null;

                        default:
                            return null;
                    }
                }

                // Primeras rutas conocidas: candidates, outputs, generations
                if (root.TryGetProperty("candidates", out var candidatesEl))
                {
                    var t = ExtractText(candidatesEl);
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                }

                if (root.TryGetProperty("outputs", out var outputsEl))
                {
                    var t = ExtractText(outputsEl);
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                }

                if (root.TryGetProperty("generations", out var gensEl))
                {
                    var t = ExtractText(gensEl);
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                }

                // Intentar extraer texto de todo el documento
                var fallback = ExtractText(root);
                if (!string.IsNullOrWhiteSpace(fallback)) return fallback;

                // Si no se encontró texto, devolvemos el JSON crudo para facilitar debugging
                return jsonResponse;
            }
            catch (JsonException)
            {
                return jsonResponse;
            }

        }
    }
}
