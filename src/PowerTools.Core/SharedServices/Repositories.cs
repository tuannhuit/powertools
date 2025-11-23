using PowerTools.Core.Models;
using System.Linq;
using System.Windows.Controls;

namespace PowerTools.Core.SharedServices
{
    public delegate void RepositoryHandler( );

    public static class Repositories
    {
        public static RepositoryRemote RepositoryRemote = new RepositoryRemote();
        public static RepositoryLocal RepositoryLocal = new RepositoryLocal();

        public static event RepositoryHandler RepositoriesChanged;

        /// <summary>
        /// Refresh the information of modules from remote and local
        /// </summary>
        public static void Refresh()
        {
            RepositoryLocal.Load();
            RepositoryRemote.Load(false);

            if (RepositoryRemote.ModuleList.Any())
            {
                foreach (var remoteModule in RepositoryRemote.ModuleList)
                {
                    var localModule = RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == remoteModule.Name);
                    if (localModule == null)
                    {
                        RepositoryLocal.ModuleList.Add(new ToolModule
                        {
                            Name = remoteModule.Name,
                            DisplayName = remoteModule.DisplayName,
                            PublisherDisplayName = remoteModule.PublisherDisplayName,
                            RepoLink = remoteModule.RepoLink,
                            Description = remoteModule.Description,
                            ExecutionName = remoteModule.ExecutionName,
                            AllVersions = remoteModule.AllVersions,
                            Icon = remoteModule.Icon,
                            IconColor = remoteModule.IconColor,
                            IconImageRelativeLocation = remoteModule.IconImageRelativeLocation
                        });
                    }
                    else
                    {
                        localModule.DisplayName = remoteModule.DisplayName;
                        localModule.PublisherDisplayName = remoteModule.PublisherDisplayName;
                        localModule.RepoLink = remoteModule.RepoLink;
                        localModule.Description = remoteModule.Description;
                        localModule.AllVersions = remoteModule.AllVersions;
                        localModule.ExecutionName = remoteModule.ExecutionName;
                        localModule.Icon = remoteModule.Icon;
                        localModule.IconColor = remoteModule.IconColor;
                        localModule.IconImageRelativeLocation = remoteModule.IconImageRelativeLocation;
                    }
                }
            }
        }

        public static void NotifyRepositoriesChanged()
        {
            RepositoriesChanged?.Invoke();
        }

        public static void Store()
        {
            RepositoryLocal.Store();
        }
    }
}
