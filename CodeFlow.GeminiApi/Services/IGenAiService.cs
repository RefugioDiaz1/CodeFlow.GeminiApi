namespace CodeFlow.GeminiApi.Services
{
    public interface IGenAiService
    {
        Task<string> GenerarPlaneacionAsync(string tema);
    }
}
