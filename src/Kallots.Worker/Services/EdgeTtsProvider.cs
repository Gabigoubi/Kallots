using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Kallots.Core.Interfaces;
using NAudio.Wave;

namespace Kallots.Worker.Services
{
    public class EdgeTtsProvider : ITtsProvider
    {
        public async Task SpeakAsync(string text)
        {
            // Creates a temporary path in the OS to store the downloaded speech
            var tempAudioPath = Path.Combine(Path.GetTempPath(), "kallots_speech.mp3");

            // 1. Call the edge-tts CLI tool asynchronously
            // Important: This assumes you have python installed and ran 'pip install edge-tts' on your desktop
            var processInfo = new ProcessStartInfo
            {
                FileName = "edge-tts",
                // 'pt-BR-AntonioNeural' is one of the highest quality Brazilian Portuguese voices
                Arguments = $"--voice pt-BR-AntonioNeural --text \"{text}\" --write-media \"{tempAudioPath}\"",
                UseShellExecute = true,
                CreateNoWindow = true // Keeps the CLI hidden from the user
            };

            using (var process = Process.Start(processInfo))
            {
                if (process != null)
                {
                    // Awaits the CLI to finish downloading the MP3 file
                    await process.WaitForExitAsync();
                }
            }

            // 2. Play the generated MP3 file
            if (File.Exists(tempAudioPath))
            {
                using var audioFile = new MediaFoundationReader(tempAudioPath);
                using var outputDevice = new WaveOutEvent();
                
                outputDevice.Init(audioFile);
                outputDevice.Play();

                // 3. Keep the thread alive while the audio is playing
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(100);
                }
                
                // 4. Memory/Disk Cleanup
                File.Delete(tempAudioPath);
            }
        }
    }
}
