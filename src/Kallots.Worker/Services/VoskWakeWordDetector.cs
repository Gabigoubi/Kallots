
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Vosk;

namespace Kallots.Worker.Services
{
    public class VoskWakeWordDetector : IWakeWordDetector
    {
        public event EventHandler? WakeWordDetected;
        private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "Vosk");
        private readonly ILogger<VoskWakeWordDetector> _logger;

        public VoskWakeWordDetector(ILogger<VoskWakeWordDetector> logger)
        {
            _logger = logger;
        }

        public Task StartListeningAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation("[DEBUG] Inicializando motor acústico do Vosk...");
                Vosk.Vosk.SetLogLevel(-1); 
                
                using var model = new Model(_modelPath);
                using var recognizer = new VoskRecognizer(model, 16000.0f);

                using var waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1)
                };

                waveIn.DataAvailable += (sender, e) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        waveIn.StopRecording();
                        return;
                    }

                    if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                    {
                        var result = recognizer.Result();
                        
                        // Data Normalization for safe analysis
                        var jsonMinusculo = result.ToLower();

                        // FUZZY MATCHING RADAR: Captures the wake word and its common hallucinations
                        if (jsonMinusculo.Contains("kallots") || 
                            jsonMinusculo.Contains("motos")   || 
                            jsonMinusculo.Contains("calotes") || 
                            jsonMinusculo.Contains("carlos")  || 
                            jsonMinusculo.Contains("calots"))
                        {
                            _logger.LogWarning("[GATILHO] Alvo detectado via Fuzzy Match! Identificado no texto bruto: {Result}", result);
                            
                            // Dispara o sinalizador para o Orquestrador iniciar a gravação do Whisper
                            WakeWordDetected?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            // Log secundário apenas para você acompanhar o ruído descartado no terminal
                            _logger.LogInformation("[Vosk Descartado] Som ambiente não reconhecido como gatilho: {Result}", result);
                        }
                    }
                };

                waveIn.StartRecording();
                _logger.LogInformation("[SUCESSO] Ouvido operacional. Aguardando palavra de ativação...");

                cancellationToken.WaitHandle.WaitOne();
                
            }, cancellationToken);
        }
    }
}