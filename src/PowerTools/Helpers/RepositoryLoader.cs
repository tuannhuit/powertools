using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PowerTools.Helpers
{
    public class RepositoryLoader
    {
        private static RepositoryLoader _instance;

        public static RepositoryLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RepositoryLoader();
                }

                return _instance;
            }
        }

        private bool _isLoaded;

        public Repository<RemoteToolModule> RemoteRepository
        {
            get;
            private set;
        }

        public Repository<ToolModule> LocalRepository
        {
            get;
            private set;
        }

        public string RepositoryLocalPath
        {
            get
            {
                var localRepositoryPath = ModuleGlobalSettings.Instance.RepositoryLocal;
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
        }

        public string RepositoryRemotePath
            => Path.Combine(ModuleGlobalSettings.Instance.RepositoryRemote, ModuleGlobalSettings.Instance.RepositoryFileName);

        private RepositoryLoader()
        {
            LocalRepository = new Repository<ToolModule>();
        }

        /// <summary>
        /// Refresh the information of modules from remote and local
        /// </summary>
        public void Refresh()
        {
            // First, load local repository information
            LoadLocalRepository();

            // Second, load remote repository information
            LoadRemoteRepository();

            if (RemoteRepository.ModuleList.Any())
            {
                foreach (var remoteModule in RemoteRepository.ModuleList)
                {
                    var localModule = LocalRepository.ModuleList.FirstOrDefault(p => p.Name == remoteModule.Name);
                    if (localModule == null)
                    {
                        LocalRepository.ModuleList.Add(new ToolModule
                        {
                            Name = remoteModule.Name,
                            Description = remoteModule.Description,
                            ExecutionName = remoteModule.ExecutionName,
                            AllVersions = remoteModule.AllVersions
                        });
                    }
                    else
                    {
                        localModule.Description = remoteModule.Description;
                        localModule.AllVersions = remoteModule.AllVersions;
                        localModule.ExecutionName = remoteModule.ExecutionName;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the local repository
        /// </summary>
        public Repository<ToolModule> LoadLocalRepository()
        {
            try
            {
                var localModulePath = RepositoryLocalPath;
                LocalRepository = LoadRepository<ToolModule>(localModulePath);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info(e.Message);
            }

            return LocalRepository;
        }

        /// <summary>
        /// Loads the remote repository
        /// </summary>
        public Repository<RemoteToolModule> LoadRemoteRepository()
        {
            try
            {
                var remoteRepoPath = RepositoryRemotePath;
                RemoteRepository = LoadRepository<RemoteToolModule>(remoteRepoPath);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info(e.Message);
            }

            return RemoteRepository;
        }

        private Repository<T> LoadRepository<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Not found repository file at {filePath}");

            var jsonContent = File.ReadAllText(filePath);
            var repo = JsonSerializer.Deserialize<Repository<T>>(jsonContent);

            return repo;
        }

        /// <summary>
        /// Store the repository into a file within the selected module
        /// </summary>
        public void Store()
        {
            LocalRepository.SelectedModuleGuid = ModuleGlobalSettings.Instance.CurrentModule?.Guid;

            File.WriteAllText(RepositoryLocalPath, JsonSerializer.Serialize(LocalRepository));
        }
    }
}
