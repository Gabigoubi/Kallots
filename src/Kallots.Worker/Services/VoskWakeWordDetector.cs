using System;
using System.Threading;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using NAudio.Wave;
using Vosk;

namespace Kallots.Worker.Services
{
    public class VoskWakeWordDetector : IWakeWordDetector
    {
        // The flare gun! This event is triggered when the wake word is heard.
        public event EventHandler? WakeWordDetected;

// vosk path
private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "Vosk");


        public Task StartListeningAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                // 1. Initialize the AI Model
                Vosk.Vosk.SetLogLevel(-1); // Disables excessive console logs
                using var model = new Model(_modelPath);
                using var recognizer = new VoskRecognizer(model, 16000.0f);

                // 2. Initialize the Microphone (16kHz, Mono is required by Vosk)
                using var waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1)
                };

                // 3. Define what happens when audio is captured
                waveIn.DataAvailable += (sender, e) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        waveIn.StopRecording();
                        return;
                    }

                    // Feed the audio buffer to Vosk
                    if (recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
                    {
                        var result = recognizer.Result();
                        
                        // Check for the wake word (including common phonetic misspellings)
                        if (result.ToLower().Contains("kallots") || result.ToLower().Contains("carlos") || result.ToLower().Contains("calots"))
                        {
                            // Fire the event!
                            WakeWordDetected?.Invoke(this, EventArgs.Empty);
                        }
                    }
                };

                // 4. Start listening
                waveIn.StartRecording();

                // Keep this background thread alive until the system shuts down
                cancellationToken.WaitHandle.WaitOne();
                
            }, cancellationToken);
        }
    }
}
