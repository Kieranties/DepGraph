using System;
using System.IO;
using NuGet.Common;
using NuGet.ProjectModel;

namespace DepGraph.Serialization
{
    public class DefaultLockFileReader : ILockFileReader
    {
        public LockFile Read(string path, ILogger logger)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Could not find file '{path}'");


            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            return LockFileUtilities.GetLockFile(path, logger);
        }
    }
}
