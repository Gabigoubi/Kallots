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
        
        // Injetando o sistema de Logs
        private readonly ILogger<VoskWakeWordDetector> _logger;

        public VoskWakeWordDetector(ILogger<VoskWakeWordDetector> logger)
        {
            _logger = logger;
        }

        public Task StartListeningAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation("Carregando modelo Vosk na memória...");
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
                        
                        // LOG CRÍTICO: Imprime o JSON bruto que o Vosk reconheceu
                        _logger.LogInformation("Vosk ouviu: {Result}", result);
                        
                        if (result.ToLower().Contains("kallots") || result.ToLower().Contains("carlos") || result.ToLower().Contains("calots"))
                        {
                            _logger.LogWarning("GATILHO ACIONADO! Palavra de ativação reconhecida.");
                            WakeWordDetected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                };

                waveIn.StartRecording();
                _logger.LogInformation("Microfone ABERTO. Vosk escutando ativamente o ambiente...");

                cancellationToken.WaitHandle.WaitOne();
                
            }, cancellationToken);
        }
    }
}