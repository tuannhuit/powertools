using System;
using PowerTools.Core.Configurations;
using System.IO;
using System.Linq;
using System.Text.Json;
using PowerTools.Core.SharedServices;

namespace PowerTools.Core.Models
{
    public class RepositoryLocal : Repository<ToolModule>
    {
        public override string GetRepositoryPath()
        {
            var localRepositoryPath = ModuleGlobalSettings.Instance.RepositoryLocalName;
            if (!Directory.Exists(localRepositoryPath))
                Directory.CreateDirectory(localRepositoryPath);

            var moduleFilePath = Path.Combine(localRepositoryPath, ModuleGlobalSettings.Instance.RepositoryFileName);

            if (!File.Exists(moduleFilePath))
            {
                File.Create(moduleFilePath).Close();
                File.WriteAllText(moduleFilePath, "{}");
            }

            return moduleFilePath;
        }

        protected override RepositoryInformation<ToolModule> AfterLoad(RepositoryInformation<ToolModule> repositoryInformation)
        {
            var uninstalledModules = repositoryInformation.ModuleList.Where(p => p.MarkedUninstalledVersions.Any()).ToList();
            foreach (var module in uninstalledModules)
            {
                var locations = module.UninstalledVersionLocations;
                foreach (var location in locations)
                {
                    if (Directory.Exists(location))
                    {
                        try
                        {
                            Directory.Delete(location, true);
                        }
                        catch (Exception e)
                        {
                            LoggingService.Instance.Error($"Failed to delete physical module '{location}'", e);
                        }
                    }
                }

                if (module.MarkedUninstalledVersions.Contains(module.Version))
                {
                    module.Version = null;
                }

                module.MarkedUninstalledVersions.Clear();
            }

            if (uninstalledModules.Any())
            {
                var repoPath = GetRepositoryPath();
                File.WriteAllText(repoPath, JsonSerializer.Serialize(repositoryInformation));
            }

            return repositoryInformation;
        }

        public override void Store()
        {
            var repoPath = GetRepositoryPath();
            File.WriteAllText(repoPath, JsonSerializer.Serialize(RepositoryInformation));
        }
    }
}
