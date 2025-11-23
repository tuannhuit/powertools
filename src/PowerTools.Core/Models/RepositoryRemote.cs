using PowerTools.Core.Configurations;
using System.IO;

namespace PowerTools.Core.Models
{
    public class RepositoryRemote : Repository<RemoteToolModule>
    {
        public override string GetRepositoryPath()
        {
            return Path.Combine(ModuleGlobalSettings.Instance.RepositoryRemote, ModuleGlobalSettings.Instance.RepositoryFileName);
        }

        public override void Store()
        {
            // TODO: Do nothing
        }
    }
}