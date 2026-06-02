using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using Kallots.Worker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kallots.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWakeWordDetector _wakeWordDetector;
        private readonly ISttProvider _sttProvider;
        private readonly ILlmProvider _llmProvider;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ITtsProvider _ttsProvider;
        private readonly AudioRecorder _audioRecorder;

        // The .NET Dependency Injection automatically provides these implementations
        public Worker(
            ILogger<Worker> logger,
            IWakeWordDetector wakeWordDetector,
            ISttProvider sttProvider,
            ILlmProvider llmProvider,
            ICommandExecutor commandExecutor,
            ITtsProvider ttsProvider)
        {
            _logger = logger;
            _wakeWordDetector = wakeWordDetector;
            _sttProvider = sttProvider;
            _llmProvider = llmProvider;
            _commandExecutor = commandExecutor;
            _ttsProvider = ttsProvider;
            
            // AudioRecorder doesn't have an interface for the MVP as it's a simple internal utility
            _audioRecorder = new AudioRecorder(); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kallots Background Service starting...");

            // 1. Boot sequence greeting requested in the MVP scope
            await _ttsProvider.SpeakAsync("Olá Usuário, tudo bem? Aguarde enquanto eu estou inicializando meus dados internos... sabe como é né, variáveis e etc... agora sim, o que vamos fazer hoje?");

            // 2. Subscribe to the Wake Word event (The flare gun)
            _wakeWordDetector.WakeWordDetected += async (sender, args) => await ProcessVoiceCommandAsync();

            // 3. Start the continuous listening loop
            await _wakeWordDetector.StartListeningAsync(stoppingToken);
        }

        // The core orchestration logic
        private async Task ProcessVoiceCommandAsync()
        {
            try
            {
                var tempAudioPath = Path.Combine(Path.GetTempPath(), "kallots_command.wav");

                // UX Feedback so you know it heard the wake word
                await _ttsProvider.SpeakAsync("Pode falar.");

                // Step 3: Record and Transcribe
                await _audioRecorder.RecordCommandAsync(tempAudioPath, 5); // Records for 5 seconds
                var userText = await _sttProvider.TranscribeAudioAsync(tempAudioPath);

                if (string.IsNullOrWhiteSpace(userText))
                    return;

                // Step 4: Process Intent with Groq
                var intent = await _llmProvider.ProcessIntentAsync(userText);

                // Step 5 & 6: Execute OS Command and Provide Voice Feedback
                if (intent != "UNKNOWN_COMMAND")
                {
                    await _ttsProvider.SpeakAsync("Ok chefe");
                    _commandExecutor.ExecuteCommand(intent);
                }
                else
                {
                    await _ttsProvider.SpeakAsync("Desculpe, não entendi o comando.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice command pipeline.");
            }
        }
    }
}
