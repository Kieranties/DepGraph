using NuGet.Common;
using NuGet.ProjectModel;

namespace DepGraph.Serialization
{
    public interface ILockFileReader
    {
        LockFile Read(string path, ILogger logger);
    }
}
