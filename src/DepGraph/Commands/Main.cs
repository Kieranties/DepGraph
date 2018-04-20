using DepGraph.Logging;
using DepGraph.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
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
    public class Main
    {
        private static readonly char[] _invalidChars = Path.GetInvalidFileNameChars();
        private readonly ILogger<Main> _logger;
        private readonly ILockFileReader _lockFileReader;

        public Main(ILogger<Main> logger, ILockFileReader lockFileReader)
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

            var targetBuilder = new NodeBuilder<LockFileTargetLibrary>(
                    x =>
                    {
                        var node = new Node {
                            Id = x.Name,
                            Label = x.Name,
                            
                        };
                        node.Properties.Add("Version", x.Version);
                        return node;
                    });

            var linkBuilder = new LinksBuilder<LockFileTargetLibrary>(
                x =>
                    x.Dependencies.Select(
                        d => new Link
                        {
                            Source = x.Name,
                            Target = d.Id
                        }
                    )
                );


            var builder = new DgmlBuilder
            {
                NodeBuilders = new NodeBuilder[] { targetBuilder },
                LinkBuilders = new LinkBuilder[] { linkBuilder }
            };

            foreach (var target in lockFile.Targets)
            {
                var graph = builder.Build(target.Libraries);

                graph.WriteToFile(Path.Combine(Destination, $"{target.Name}.dgml"));
            }
        }
    }
}
