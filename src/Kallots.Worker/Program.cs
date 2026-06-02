using Kallots.Core.Interfaces;
using Kallots.Worker;
using Kallots.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Creates the builder for the worker service
var builder = Host.CreateApplicationBuilder(args);

// Configures the application to run as a native Windows Background Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Kallots Assistant Service";
});

// Setting up the Dependency Injection Container (The "HR Panel")
// "Whenever a class asks for an interface, provide this specific implementation"
builder.Services.AddSingleton<IWakeWordDetector, VoskWakeWordDetector>();
builder.Services.AddSingleton<ISttProvider, WhisperSttProvider>();
builder.Services.AddSingleton<ILlmProvider, GroqLlmProvider>();
builder.Services.AddSingleton<ICommandExecutor, CommandExecutor>();
builder.Services.AddSingleton<ITtsProvider, EdgeTtsProvider>();

// Registers the native HTTP Client required by the Groq Llm Provider
builder.Services.AddHttpClient();

// Registers the main orchestration loop
builder.Services.AddHostedService<Worker>();

// Builds and runs the application
var host = builder.Build();
host.Run();
