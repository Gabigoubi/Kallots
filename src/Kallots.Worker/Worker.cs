
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
            
            // Task 2.1: Definição da Máquina de Estados e condição de fuga (Anti-Deadlock)
            int maxAttempts = 2;
            int currentAttempt = 1;
            bool commandExecuted = false;

            try
            {
                var tempAudioPath = Path.Combine(Path.GetTempPath(), "kallots_command.wav");
                _logger.LogInformation(">>> GATILHO RECEBIDO. INICIANDO PIPELINE (MÁQUINA DE ESTADOS) <<<");
                
                // Feedback Sonoro UX na primeira tentativa
                if (OperatingSystem.IsWindows())
                {
                    Console.Beep(800, 250); 
                }

                while (currentAttempt <= maxAttempts && !commandExecuted)
                {
                    _logger.LogInformation("--- TENTATIVA {Attempt}/{MaxAttempts} ---", currentAttempt, maxAttempts);
                    
                    // Task 2.3: Gravação Curta na clarificação (5s no gatilho inicial, 3s no loop)
                    int recordDuration = (currentAttempt == 1) ? 5 : 3;
                    
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
                        _logger.LogWarning("Whisper não captou nada válido (Silêncio ou ruído).");
                        if (currentAttempt < maxAttempts)
                        {
                            await _ttsProvider.SpeakAsync("Não consegui ouvir. Pode repetir o nome do programa?");
                        }
                        currentAttempt++;
                        continue; // Pula as próximas etapas e reinicia a gravação
                    }
                    
                    _logger.LogInformation("   Texto Mapeado: [{UserText}]", userText);

                    stepTimer.Restart();
                    _logger.LogInformation("3. Disparando para Inteligência Nuvem (Groq API)...");
                    var intent = await _llmProvider.ProcessIntentAsync(userText);
                    _logger.LogInformation("   [Telemetria] Tempo de Resposta da API: {Elapsed}ms", stepTimer.ElapsedMilliseconds);
                    _logger.LogInformation("   Intenção Retornada: [{Intent}]", intent);

                    _logger.LogInformation("4. Acionando Motor de Execução Windows...");
                    
                    // O novo CommandExecutor agora retorna um bool validando se achou o atalho
                    commandExecuted = _commandExecutor.ExecuteCommand(intent);

                    if (commandExecuted)
                    {
                        await _ttsProvider.SpeakAsync("Ok chefe, abrindo.");
                        _logger.LogInformation("Ação executada no SO com sucesso!");
                    }
                    else
                    {
                        _logger.LogWarning("Intenção inválida ou aplicativo não encontrado no disco.");
                        
                        // Task 2.2: Feedback Inteligente e acionamento do Loop
                        if (currentAttempt < maxAttempts)
                        {
                            await _ttsProvider.SpeakAsync("Não encontrei esse aplicativo, ou não entendi. Qual programa você quer abrir?");
                        }
                        else
                        {
                            await _ttsProvider.SpeakAsync("Ainda não consegui encontrar. Operação cancelada.");
                            _logger.LogWarning("Limite de tentativas atingido. Abortando loop de conversação.");
                        }
                    }

                    currentAttempt++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal processando o pipeline de voz.");
                await _ttsProvider.SpeakAsync("Ocorreu um erro interno. Consulte o log do sistema.");
            }
            finally
            {
                // Limpa os tempos e destranca o assistente com segurança
                globalTimer.Stop();
                _logger.LogInformation(">>> PIPELINE ENCERRADO. Tempo Total: {Elapsed}ms. Destrancando audição. <<<", globalTimer.ElapsedMilliseconds);
                _isProcessingCommand = false;
            }
        }
    }
}
