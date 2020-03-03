using DepGraph.Commands;
using DepGraph.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DepGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            // Build services
            var services = new ServiceCollection()
                .AddSingleton<ILockFileReader, DefaultLockFileReader>()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();
            
            // Invoke the command
            var app = new CommandLineApplication<Graph>();
            app
                .Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            app.Execute(args);
        }
    }
}
