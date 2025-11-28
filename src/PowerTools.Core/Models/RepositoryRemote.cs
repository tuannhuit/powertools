using PowerTools.Core.Configurations;
using System.IO;
using System.Net;
using System.Net.Http;
using PowerTools.Core.SharedServices;

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

        protected override RepositoryInformation<RemoteToolModule> ReadRepositoryInformation(string repositoryPath)
        {
            if (File.Exists(repositoryPath))
            {
                return base.ReadRepositoryInformation(repositoryPath);
            }

            var localTempFilePath = Path.Combine(ApplicationService.Instance.GetOrCreateTempFolder(), "remoteRepository.json");

            using var webClient = new WebClient();
            webClient.DownloadFile(repositoryPath, localTempFilePath);

            if (File.Exists(localTempFilePath))
            {
                return base.ReadRepositoryInformation(localTempFilePath);
            }
            else
            {
                return null;
            }
        }
    }
}