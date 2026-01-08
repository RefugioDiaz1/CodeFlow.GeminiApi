using System.Threading.Tasks;

namespace CodeFlow.GeminiApi.Services
{
 

    public class MockGenAiService : CodeFlow.GeminiApi.Services.IGenAiService
    {
        public Task<string> GenerarPlaneacionAsync(string tema)
        {
            // Respuesta simulada para pruebas
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Planeación simulada para: {tema}");
           
            return Task.FromResult(sb.ToString());
        }
    }

}
