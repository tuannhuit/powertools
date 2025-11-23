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
            // Remove all local modules marked as Deleted
            // After storing modules

            var markDeletedModules = repositoryInformation.ModuleList.Where(p => p.IsMarkDeleted).ToArray();

            if (markDeletedModules.Any())
            {
                for (int i = 0; i < markDeletedModules.Length; i++)
                {
                    if (Directory.Exists(markDeletedModules[i].ModuleLocation))
                    {
                        try
                        {
                            Directory.Delete(markDeletedModules[i].ModuleLocation, true);
                        }
                        catch (Exception e)
                        {
                            LoggingService.Instance.Error($"Failed to delete physical module '{markDeletedModules[i].ModuleLocation}'", e);
                        }
                    }

                    markDeletedModules[i].IsMarkDeleted = false;
                    //repositoryInformation.ModuleList.Remove(markDeletedModules[i]);
                }

                Store();
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
