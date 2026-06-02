using System;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Kallots.Worker.Services
{
    public class AudioRecorder
    {
        // Records audio from the default microphone for a specified duration
        public async Task RecordCommandAsync(string outputFilePath, int durationInSeconds = 5)
        {
            // 1. Setup the input (Microphone). 
            // 16000 Hz, 1 Channel (Mono) is the standard required by Whisper
            using var waveSource = new WaveInEvent
            {
                WaveFormat = new WaveFormat(16000, 1) 
            };

            // 2. Setup the output (File on disk)
            using var waveFile = new WaveFileWriter(outputFilePath, waveSource.WaveFormat);

            // 3. Define what happens when audio data comes in: write it to the file
            waveSource.DataAvailable += (sender, e) =>
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            };

            // 4. Start recording
            waveSource.StartRecording();

            // 5. Wait for the specified duration (e.g., 5 seconds) asynchronously
            // This prevents the system from freezing while recording
            await Task.Delay(durationInSeconds * 1000);

            // 6. Stop recording. The 'using' blocks will automatically save and close the file.
            waveSource.StopRecording();
        }
    }
}
