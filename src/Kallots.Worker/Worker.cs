
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
            // Aciona a tranca de segurança
            _isProcessingCommand = true;
            var globalTimer = Stopwatch.StartNew();

            try
            {
                var tempAudioPath = Path.Combine(Path.GetTempPath(), "kallots_command.wav");

                _logger.LogInformation(">>> GATILHO RECEBIDO. INICIANDO PIPELINE <<<");
                
                // Feedback Sonoro UX (Earcon): Bip instantâneo nativo do Windows (800 Hz por 250ms)
                // Substitui a lentidão do TTS "Pode Falar"
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 250); 
                }

                var stepTimer = Stopwatch.StartNew();
                _logger.LogInformation("1. Gravando 5 segundos de áudio do microfone...");
                await _audioRecorder.RecordCommandAsync(tempAudioPath, 5); 
                _logger.LogInformation("   [Telemetria] Tempo de Gravação: {Elapsed}ms", stepTimer.ElapsedMilliseconds);
                
                stepTimer.Restart();
                _logger.LogInformation("2. Transcrevendo áudio pesado com Whisper local...");
                var userText = await _sttProvider.TranscribeAudioAsync(tempAudioPath);
                _logger.LogInformation("   [Telemetria] Tempo de Transcrição: {Elapsed}ms", stepTimer.ElapsedMilliseconds);

                if (string.IsNullOrWhiteSpace(userText))
                {
                    _logger.LogWarning("Whisper não entendeu nada. Abortando fluxo para evitar consumo nulo.");
                    return; // Cai direto pro bloco 'finally'
                }
                
                _logger.LogInformation("   Texto Mapeado: [{UserText}]", userText);

                stepTimer.Restart();
                _logger.LogInformation("3. Disparando para Inteligência Nuvem (Groq API)...");
                var intent = await _llmProvider.ProcessIntentAsync(userText);
                _logger.LogInformation("   [Telemetria] Tempo de Resposta da API: {Elapsed}ms", stepTimer.ElapsedMilliseconds);
                _logger.LogInformation("   Intenção Retornada: [{Intent}]", intent);

                _logger.LogInformation("4. Acionando Motor de Execução Windows...");
                if (intent != "UNKNOWN_COMMAND")
                {
                    await _ttsProvider.SpeakAsync("Ok chefe");
                    _commandExecutor.ExecuteCommand(intent);
                    _logger.LogInformation("Ação executada no SO com sucesso!");
                }
                else
                {
                    _logger.LogWarning("Nenhuma intenção conhecida mapeada.");
                    await _ttsProvider.SpeakAsync("Desculpe, não entendi o comando.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal processando o pipeline de voz.");
            }
            finally
            {
                // Limpa os tempos e destranca o assistente independentemente de ter dado erro ou não
                globalTimer.Stop();
                _logger.LogInformation(">>> PIPELINE ENCERRADO. Tempo Total: {Elapsed}ms. Destrancando audição. <<<", globalTimer.ElapsedMilliseconds);
                _isProcessingCommand = false;
            }
        }
    }
}