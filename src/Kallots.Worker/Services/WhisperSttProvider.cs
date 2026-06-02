
using System;
using System.IO;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using Whisper.net;

namespace Kallots.Worker.Services
{
    public class WhisperSttProvider : ISttProvider
    {
        // DYNAMIC PATH INJECTED HERE
        private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "Whisper", "ggml-base.bin");
        private readonly WhisperFactory _whisperFactory;

        public WhisperSttProvider()
        {
            // We load the factory in the constructor.
            // This keeps the 70MB model in RAM continuously, drastically reducing latency
            // when the user actually speaks, as we don't need to load it from the SSD every time.
            _whisperFactory = WhisperFactory.FromPath(_modelPath);
        }

        public async Task<string> TranscribeAudioAsync(string audioFilePath)
        {
            if (!File.Exists(audioFilePath))
            {
                return string.Empty;
            }

            // The 'using' keyword guarantees that the C++ unmanaged memory
            // is safely destroyed and returned to the OS after transcription.
            using var processor = _whisperFactory.CreateBuilder()
                .WithLanguage("pt") // Forces Portuguese to skip language detection and speed up processing
                .Build();

            using var fileStream = File.OpenRead(audioFilePath);
            
            string fullTranscript = "";

            // Whisper processes audio in small segments
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                fullTranscript += result.Text + " ";
            }

            return fullTranscript.Trim();
        }
    }
}
