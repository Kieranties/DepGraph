using DepGraph.Logging;
using DepGraph.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace DepGraph.Commands
{
    [HelpOption]
    [Command("Graph", Description = "Creates DGML files for a dotnet project")]
    public class Graph
    {
        private static readonly char[] _invalidChars = Path.GetInvalidFileNameChars();
        private readonly ILogger<Graph> _logger;
        private readonly ILockFileReader _lockFileReader;

        public Graph(ILogger<Graph> logger, ILockFileReader lockFileReader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lockFileReader = lockFileReader ?? throw new ArgumentNullException(nameof(lockFileReader));
        }

        [Argument(0, Description = "The source file to read.")]
        [Required]
        public string Source { get; set; }

        [Option("-d|--destination <PATH>", Description = "The output path for the dgml file.")]
        public string Destination { get; set; } = Environment.CurrentDirectory;

        public void OnExecute(CommandLineApplication app)
        {
            var lockFile = _lockFileReader.Read(Source, new NugetLoggerWrapper(_logger));


            var spec = lockFile.PackageSpec;

            var packageSpecBuilder = new NodeBuilder<PackageSpec>(
                x => new Node
                {
                    Id = x.Name
                });

            var libBuilder = new NodeBuilder<LibraryDependency>(
                x => new Node
                {
                    Id = x.Name // TODO category based on reference type
                });

            var linkBuilder = new LinkBuilder<LibraryDependency>(
                x => new Link
                {
                    Source = spec.Name,
                    Target = x.Name,
                    Label = x.LibraryRange.VersionRange.ToNormalizedString()
                });

            var builder = new DgmlBuilder
            {
                NodeBuilders = new NodeBuilder[] {
                    packageSpecBuilder,
                    libBuilder
                },
                LinkBuilders = new LinkBuilder[] {
                    linkBuilder
                }
            };

            foreach (var fw in spec.TargetFrameworks)
            {
                var graph = builder.Build(fw.Dependencies, new[] { spec });
                graph.WriteToFile(Path.Combine(Destination, $"{fw.FrameworkName}.dgml"));
            }
        }
    }
}
