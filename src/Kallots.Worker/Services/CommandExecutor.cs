using System;
using System.Diagnostics;
using Kallots.Core.Interfaces;

namespace Kallots.Worker.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        public void ExecuteCommand(string commandIntent)
        {
            var intent = commandIntent.Trim().ToUpper();

            try
            {
                switch (intent)
                {
                    case "OPEN_VSCODE":
                        StartOsProcess("code");
                        break;
                    case "OPEN_BROWSER":
                        StartOsProcess("https://www.google.com");
                        break;
                    case "OPEN_CALCULATOR":
                        StartOsProcess("calc.exe");
                        break;
                    case "UNKNOWN_COMMAND":
                        break;
                    default:
                        break;
                }
            }
            catch (Exception) // <-- Removed the 'ex' variable here
            {
                // We catch the exception gracefully so the service doesn't crash
            }
        }

        private void StartOsProcess(string processName)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = processName,
                UseShellExecute = true 
            };

            Process.Start(processInfo);
        }
    }
}