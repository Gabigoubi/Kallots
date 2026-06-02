using System.Threading.Tasks;

namespace Kallots.Core.Interfaces
{
    public interface ITtsProvider
    {
        // Sends text to Edge TTS and plays the generated audio
        Task SpeakAsync(string text);
    }
}
