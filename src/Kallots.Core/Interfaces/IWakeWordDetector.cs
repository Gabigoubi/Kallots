using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kallots.Core.Interfaces
{
    public interface IWakeWordDetector
    {
        // Event triggered when the user says "Kallots"
        event EventHandler WakeWordDetected;
        
        // Starts the continuous listening loop
        Task StartListeningAsync(CancellationToken cancellationToken);
    }
}
