
using System;
using System.Diagnostics; 
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
        
        // Tranca de Arquitetura: Impede que o sistema tente rodar dois comandos ao mesmo tempo
        private bool _isProcessingCommand = false; 

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

        await _ttsProvider.SpeakAsync("Olá Gabriel, tudo bem? Aguarde enquanto eu estou inicializando meus dados internos... sabe como é, né? variáveis e COISA e tals.. agora sim, quando precisar de mim, diga \"Kallots\".");            // Evento do Vosk: Só iniciamos se o assistente estiver livre
            _wakeWordDetector.WakeWordDetected += async (sender, args) => 
            {
                if (!_isProcessingCommand)
                {
                    await ProcessVoiceCommandAsync();
                }
            };

            await _wakeWordDetector.StartListeningAsync(stoppingToken);
        }

private async Task ProcessVoiceCommandAsync()
        {
            _isProcessingCommand = true;
            var globalTimer = Stopwatch.StartNew();
            
            int maxAttempts = 2;
            int currentAttempt = 1;
            bool commandExecuted = false;

            try
            {
                _logger.LogInformation(">>> GATILHO RECEBIDO. INICIANDO PIPELINE (MÁQUINA DE ESTADOS) <<<");
                
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 250); 
                }

                while (currentAttempt <= maxAttempts && !commandExecuted)
                {
                    _logger.LogInformation("--- TENTATIVA {Attempt}/{MaxAttempts} ---", currentAttempt, maxAttempts);
                    
                    // CORREÇÃO: Geramos um nome de arquivo ÚNICO para cada tentativa para evitar colisão de I/O
                    var tempAudioPath = Path.Combine(Path.GetTempPath(), $"kallots_cmd_{Guid.NewGuid()}.wav");
                    
                    int recordDuration = (currentAttempt == 1) ? 5 : 3;
                    
                    try 
                    {
                        var stepTimer = Stopwatch.StartNew();
                        _logger.LogInformation("1. Gravando {Duration} segundos de áudio do microfone...", recordDuration);
                        await _audioRecorder.RecordCommandAsync(tempAudioPath, recordDuration); 
                        _logger.LogInformation("   [Telemetria] Tempo de Gravação: {Elapsed}ms", stepTimer.ElapsedMilliseconds);
                        
                        stepTimer.Restart();
                        _logger.LogInformation("2. Transcrevendo áudio pesado com Whisper local...");
                        var userText = await _sttProvider.TranscribeAudioAsync(tempAudioPath);
                        _logger.LogInformation("   [Telemetria] Tempo de Transcrição: {Elapsed}ms", stepTimer.ElapsedMilliseconds);

                        if (string.IsNullOrWhiteSpace(userText))
                        {
                            _logger.LogWarning("Whisper não captou nada válido.");
                            if (currentAttempt < maxAttempts) await _ttsProvider.SpeakAsync("Não ouvi. Pode repetir?");
                            currentAttempt++;
                            continue; 
                        }
                        
                        _logger.LogInformation("   Texto Mapeado: [{UserText}]", userText);

                        stepTimer.Restart();
                        _logger.LogInformation("3. Disparando para Inteligência Nuvem (Groq API)...");
                        var intent = await _llmProvider.ProcessIntentAsync(userText);
                        
                        commandExecuted = _commandExecutor.ExecuteCommand(intent);

                        if (commandExecuted)
                        {
                            await _ttsProvider.SpeakAsync("Ok chefe, abrindo.");
                        }
                        else
                        {
                            if (currentAttempt < maxAttempts) await _ttsProvider.SpeakAsync("Não entendi. Qual programa quer abrir?");
                            else await _ttsProvider.SpeakAsync("Operação cancelada.");
                        }
                    }
                    finally
                    {
                        // CORREÇÃO: Limpeza de disco. Remove o arquivo temporário após o uso para não lotar o HD
                        if (File.Exists(tempAudioPath))
                        {
                            File.Delete(tempAudioPath);
                        }
                    }

                    currentAttempt++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal processando o pipeline de voz.");
                await _ttsProvider.SpeakAsync("Ocorreu um erro interno.");
            }
            finally
            {
                globalTimer.Stop();
                _logger.LogInformation(">>> PIPELINE ENCERRADO. Tempo Total: {Elapsed}ms. <<<", globalTimer.ElapsedMilliseconds);
                _isProcessingCommand = false;
            }
        }
    }
}
