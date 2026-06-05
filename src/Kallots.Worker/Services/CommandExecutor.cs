using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Kallots.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Kallots.Worker.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ILogger<CommandExecutor> _logger;

        public CommandExecutor(ILogger<CommandExecutor> logger)
        {
            _logger = logger;
        }

        public bool ExecuteCommand(string commandIntent)
        {
            if (commandIntent == "UNKNOWN_COMMAND")
            {
                return false;
            }

            try
            {
                // Parse da intenção estruturada (Ex: "OPEN_APP: Visual Studio Code")
                if (commandIntent.StartsWith("OPEN_APP:"))
                {
                    var appName = commandIntent.Split(':')[1].Trim();
                    return TryOpenApplication(appName);
                }

                _logger.LogWarning("Comando não suportado pela engine atual: {Intent}", commandIntent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao tentar executar o comando: {Intent}", commandIntent);
                return false;
            }
        }

        private bool TryOpenApplication(string requestedAppName)
        {
            // Pastas alvo para a varredura
            var targetFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), // Start Menu Público
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),       // Start Menu do Usuário
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop)          // Área de Trabalho
            };

            foreach (var folder in targetFolders)
            {
                if (!Directory.Exists(folder)) continue;

                // Varredura recursiva de arquivos .lnk (atalhos do Windows)
                var shortcuts = Directory.GetFiles(folder, "*.lnk", SearchOption.AllDirectories);

                foreach (var shortcut in shortcuts)
                {
                    var shortcutName = Path.GetFileNameWithoutExtension(shortcut);

                    // Algoritmo de Match (OrdinalIgnoreCase para ignorar case e melhorar a tolerância)
                    // Um Fuzzy Match mais complexo (Levenshtein) poderia ser implementado aqui no futuro.
                    if (shortcutName.Contains(requestedAppName, StringComparison.OrdinalIgnoreCase) ||
                        requestedAppName.Contains(shortcutName, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("Match encontrado! App: {Requested} -> Atalho: {Shortcut}", requestedAppName, shortcutName);
                        StartOsProcess(shortcut);
                        return true;
                    }
                }
            }

            _logger.LogWarning("Nenhum atalho encontrado para o aplicativo: {App}", requestedAppName);
            return false;
        }

        private void StartOsProcess(string filePath)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true 
            };

            Process.Start(processInfo);
        }
    }
}
