using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kallots.Worker.Services
{
    public class GroqLlmProvider : ILlmProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GroqLlmProvider> _logger;

        public GroqLlmProvider(HttpClient httpClient, IConfiguration configuration, ILogger<GroqLlmProvider> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GroqApiKey"] ?? throw new ArgumentNullException("GroqApiKey is missing in configuration.");
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> ProcessIntentAsync(string userText)
        {
            // O C# cria um objeto anônimo seguro
            var payload = new
            {
                model = "llama-3.1-8b-instant",
                temperature = 0.0, // Zero criatividade, queremos respostas precisas
                messages = new[]
{
    new { 
        role = "system", 
        content = "You are a strict Windows desktop intent parser. If the user wants to open an application, return EXACTLY in this format: 'OPEN_APP: [App Name]'. Example: 'OPEN_APP: Spotify'. If the user's text is not about opening an app, or is unclear, return 'UNKNOWN_COMMAND'. DO NOT return conversational text, greetings, or markdown." 
    },
    new { role = "user", content = userText }
}
            };

            // O JsonSerializer cuida de escapar as aspas e caracteres especiais do "aprendizinho atualista"
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", jsonContent);
            
            if (!response.IsSuccessStatusCode)
            {
                // Se der erro de novo, agora nós vamos imprimir o motivo EXATO no log antes de quebrar
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Groq API rejeitou o pacote. Detalhes: {ErrorDetails}", errorBody);
                response.EnsureSuccessStatusCode(); 
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            
            // Navegamos pelo JSON de resposta com segurança
            using var document = JsonDocument.Parse(responseBody);
            var intent = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return intent?.Trim().ToUpper() ?? "UNKNOWN_COMMAND";
        }
    }
}
