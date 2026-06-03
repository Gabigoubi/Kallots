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
            
            _audioRecorder = new AudioRecorder(); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Kallots Background Service starting...");

            await _ttsProvider.SpeakAsync("Olá Usuário, tudo bem? Aguarde enquanto eu estou inicializando meus dados internos... sabe como é né, variáveis e etc... agora sim, o que vamos fazer hoje?");

            _wakeWordDetector.WakeWordDetected += async (sender, args) => await ProcessVoiceCommandAsync();

            await _wakeWordDetector.StartListeningAsync(stoppingToken);
        }

        private async Task ProcessVoiceCommandAsync()
        {
            try
            {
                var tempAudioPath = Path.Combine(Path.GetTempPath(), "kallots_command.wav");

                _logger.LogInformation(">>> INICIANDO PIPELINE DE COMANDO <<<");
                await _ttsProvider.SpeakAsync("Pode falar.");

                _logger.LogInformation("1. Gravando 5 segundos de áudio...");
                await _audioRecorder.RecordCommandAsync(tempAudioPath, 5); 
                
                _logger.LogInformation("2. Áudio gravado. Enviando para o Whisper converter em texto...");
                var userText = await _sttProvider.TranscribeAudioAsync(tempAudioPath);

                if (string.IsNullOrWhiteSpace(userText))
                {
                    _logger.LogWarning("Whisper retornou um texto vazio. Abortando comando.");
                    return;
                }
                
                _logger.LogInformation("   Texto reconhecido pelo Whisper: [{UserText}]", userText);

                _logger.LogInformation("3. Enviando texto para o LLM processar intenção...");
                var intent = await _llmProvider.ProcessIntentAsync(userText);
                _logger.LogInformation("   Intenção retornada pelo Groq: [{Intent}]", intent);

                _logger.LogInformation("4. Executando ação no Sistema Operacional...");
                if (intent != "UNKNOWN_COMMAND")
                {
                    await _ttsProvider.SpeakAsync("Ok chefe");
                    _commandExecutor.ExecuteCommand(intent);
                    _logger.LogInformation("Ação executada com sucesso!");
                }
                else
                {
                    _logger.LogWarning("Comando desconhecido ou não mapeado pelo LLM.");
                    await _ttsProvider.SpeakAsync("Desculpe, não entendi o comando.");
                }
                
                _logger.LogInformation(">>> PIPELINE FINALIZADO. Voltando a escutar. <<<");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal processando o comando de voz.");
            }
        }
    }
}