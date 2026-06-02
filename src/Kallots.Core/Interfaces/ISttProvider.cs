using System.Threading.Tasks;

namespace Kallots.Core.Interfaces
{
    public interface ISttProvider
    {
        // Receives the path of the recorded audio file and returns the transcribed text
        Task<string> TranscribeAudioAsync(string audioFilePath);
    }
}
