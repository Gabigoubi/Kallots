using System.Threading.Tasks;

namespace Kallots.Core.Interfaces
{
    public interface ILlmProvider
    {
        // Sends the transcribed text to Groq/LLaMA and returns the processed intent
        Task<string> ProcessIntentAsync(string userText);
    }
}
