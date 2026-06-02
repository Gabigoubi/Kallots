namespace Kallots.Core.Interfaces
{
    public interface ICommandExecutor
    {
        // Executes OS level commands (e.g., opening applications) based on the LLM intent
        // This is synchronous (void) as firing a process usually doesn't need to block the thread
        void ExecuteCommand(string commandIntent);
    }
}
