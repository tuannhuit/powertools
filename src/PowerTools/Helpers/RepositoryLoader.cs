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

        private Repository<ToolModule> _localRepository;
        private Repository<RemoteToolModule> _remoteRepository;

        /// <summary>
        /// Local repository
        /// </summary>
        public Repository<ToolModule> LocalRepository => _localRepository;

        public string RepositoryLocalPath
        {
            get
            {
                var localRepositoryPath = ModuleGlobalSettings.Instance.RepositoryLocal;
                var repositoryFileName = ModuleGlobalSettings.Instance.RepositoryFileName;

                if (!Directory.Exists(localRepositoryPath))
                    Directory.CreateDirectory(localRepositoryPath);

                var moduleFilePath = Path.Combine(localRepositoryPath, repositoryFileName);

                if (!File.Exists(moduleFilePath))
                {
                    File.Create(moduleFilePath).Close();
                    File.WriteAllText(moduleFilePath, "{}");
                }

                return moduleFilePath;
            }
        }

        public string RepositoryRemotePath => Path.Combine(ModuleGlobalSettings.Instance.RepositoryRemote, ModuleGlobalSettings.Instance.RepositoryFileName);

        private RepositoryLoader()
        {
            _localRepository = new Repository<ToolModule>();
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

            if (_remoteRepository.ModuleList.Any())
            {
                foreach (var remoteModule in _remoteRepository.ModuleList)
                {
                    var localModule = _localRepository.ModuleList.FirstOrDefault(p => p.Name == remoteModule.Name);
                    if (localModule == null)
                    {
                        _localRepository.ModuleList.Add(new ToolModule
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
            var localModulePath = RepositoryLocalPath;
            var repo = new Repository<ToolModule>();

            try
            {
                repo = LoadRepository<ToolModule>(localModulePath);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info(e.Message);
            }

            _localRepository = repo;

            return _localRepository;
        }

        /// <summary>
        /// Loads the remote repository
        /// </summary>
        public Repository<RemoteToolModule> LoadRemoteRepository()
        {
            var remoteRepoPath = RepositoryRemotePath;
            var repo = new Repository<RemoteToolModule>();

            try
            {
                repo = LoadRepository<RemoteToolModule>(remoteRepoPath);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info(e.Message);
            }

            _remoteRepository = repo;

            return _remoteRepository;
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
            var localModulePath = RepositoryLocalPath;

            _localRepository.SelectedModule = ModuleGlobalSettings.Instance.CurrentModule;
            File.WriteAllText(localModulePath, JsonSerializer.Serialize(_localRepository));
        }
    }
}
