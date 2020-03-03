using DepGraph.Logging;
using DepGraph.Serialization;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Analyses;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace DepGraph.Commands
{
    [HelpOption]
    [Command("Graph", Description = "Creates DGML files for a dotnet project")]
    public class Graph
    {
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

        [Option("-i", Description = "Immediates only")]
        public bool ImmediateOnly { get; set; } = false;

        [Option("-d|--destination <PATH>", Description = "The output path for the dgml file.")]
        public string Destination { get; set; } = Environment.CurrentDirectory;

        [Option("-n", Description = "The name for the dgml file.")]
        public string Name { get; set; } = "generated";

        [Option("-m", Description = "Write a map instead of a dgml")]
        public bool Map { get; set; } = false;

        public void OnExecute(CommandLineApplication app)
        {
            var lockFile = _lockFileReader.Read(Source, new NugetLoggerWrapper(_logger));
            if(Map)
            {
                BuildMap(lockFile);
                return;
            }

            var spec = lockFile.PackageSpec;

            var directRefs = lockFile.ProjectFileDependencyGroups[0].Dependencies.Select(x => x.Split(' ')[0]).ToArray();

            var deps = lockFile.Targets.First().Libraries;

            if (ImmediateOnly)
            {
                var packages = spec.TargetFrameworks.First().Dependencies;
                var packageNames = packages.Select(d => d.Name).ToArray();
                deps = deps
                    .Where(l => packageNames.Contains(l.Name) || l.Type == "project").ToArray();
            }

            BuildGraph(directRefs, spec, deps);
        }

        private void BuildMap(LockFile lockFile)
        {
            var countMap = lockFile.ProjectFileDependencyGroups[0]
                .Dependencies
                .Select(x => x.Split(' ')[0])
                .ToDictionary(x => x, x => 0);

            foreach (var dep in lockFile.Targets.First().Libraries)
            {
                foreach(var reference in dep.Dependencies)
                {
                    if (countMap.Keys.Contains(reference.Id))
                    {
                        countMap[reference.Id]++;
                    }
                }
            }

            using var writer = new StreamWriter(Path.Combine(Destination, $"{Name}.txt"));
            foreach(var kvp in countMap)
            {
                var line = $"{kvp.Value}\t{kvp.Key}";
                Console.WriteLine(line);
                writer.WriteLine(line);
            }

        }

        private void BuildGraph(string[] directRefs, PackageSpec spec, IList<LockFileTargetLibrary> libs)
        {
            var packageSpecBuilder = new NodeBuilder<PackageSpec>(
                x => new Node
                {
                    Id = x.Name
                });

            var directrefBuilder = new LinksBuilder<PackageSpec>(
                x => x.TargetFrameworks[0].Dependencies
                .Select(d => new Link
                {
                    Source = x.Name,
                    Target = d.Name
                }));

            var libBuilder = new NodeBuilder<LockFileTargetLibrary>(x => new Node
            {
                Id = x.Name,
                Label = $"{x.Name} [{x.Version.ToNormalizedString()}]",
                Category = x.Type
            });

            var linkBuilder = new LinksBuilder<LockFileTargetLibrary>(
                x =>
                {
                    var result = new List<Link>();

                    if (directRefs.Contains(x.Name))
                    {
                        result.Add(new Link
                        {
                            Source = spec.Name,
                            Target = x.Name
                        });
                    }

                    foreach (var dep in x.Dependencies)
                    {
                        result.Add(new Link
                        {
                            Source = x.Name,
                            Target = dep.Id,
                            Label = dep.VersionRange.ToNormalizedString()
                        });
                    }

                    return result;
                });

            var categoryBuilder = new CategoryBuilder<string>(x => new Category { Id = x, Label = x });


            var builder = new DgmlBuilder(new CategoryColorAnalysis())
            {
                NodeBuilders = new NodeBuilder[] { packageSpecBuilder, libBuilder },
                LinkBuilders = new LinkBuilder[] { linkBuilder, directrefBuilder },
                CategoryBuilders = new CategoryBuilder[] { categoryBuilder }
            };

            var graph = builder.Build(libs, new object[] { spec }, new object[] { "project", "package" });
            graph.WriteToFile(Path.Combine(Destination, $"{Name}.dgml"));
        }
    }
}
