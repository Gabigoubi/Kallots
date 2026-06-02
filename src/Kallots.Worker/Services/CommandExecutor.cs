using System;
using System.Diagnostics;
using Kallots.Core.Interfaces;

namespace Kallots.Worker.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        public void ExecuteCommand(string commandIntent)
        {
            // Normalizing the string to prevent case sensitivity bugs (e.g., open_vscode vs OPEN_VSCODE)
            var intent = commandIntent.Trim().ToUpper();

            try
            {
                // Mapping LLM Intents to actual Windows OS commands
                switch (intent)
                {
                    case "OPEN_VSCODE":
                        // 'code' is the standard CLI command registered by VS Code on Windows
                        StartOsProcess("code");
                        break;
                    
                    case "OPEN_BROWSER":
                        // Launching a URL natively forces Windows to open the default web browser
                        StartOsProcess("https://www.google.com");
                        break;
                        
                    case "OPEN_CALCULATOR":
                        StartOsProcess("calc.exe");
                        break;

                    case "UNKNOWN_COMMAND":
                        // The LLM didn't understand the user's audio.
                        // We do nothing on the OS level, but later the TTS can say "I didn't understand".
                        break;

                    default:
                        // Fallback for intents that the LLM generated but we haven't mapped yet
                        break;
                }
            }
            catch (Exception ex)
            {
                // If a program is not installed, the OS throws an error.
                // We catch it so the Kallots background service doesn't crash completely.
            }
        }

        // Helper method to wrap the boilerplate process execution code
        private void StartOsProcess(string processName)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = processName,
                // UseShellExecute = true is crucial. It asks the Windows Shell to resolve the command,
                // behaving exactly like the "Run" (Win + R) dialog.
                UseShellExecute = true 
            };

            Process.Start(processInfo);
        }
    }
}
