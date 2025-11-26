using PowerTools.Core.Configurations;
using System.IO;

namespace PowerTools.Core.Models
{
    public class RepositoryRemote : Repository<RemoteToolModule>
    {
        public override string GetRepositoryPath()
        {
            // By default, the repository would be a specific URL file path
            return Path.Combine(ModuleGlobalSettings.Instance.RepositoryRemote);
        }

        public override void Store()
        {
            // TODO: Do nothing
        }
    }
}