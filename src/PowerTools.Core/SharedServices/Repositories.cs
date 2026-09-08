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
                            OwnerName = remoteModule.OwnerName,
                            RepoName = remoteModule.RepoName,
                            RepoLink = remoteModule.RepoLink,
                            RepoType = remoteModule.RepoType,
                            Description = remoteModule.Description,
                            ExecutionName = remoteModule.ExecutionName,
                            Icon = remoteModule.Icon,
                            IconColor = remoteModule.IconColor,
                            IconImageRelativeLocation = remoteModule.IconImageRelativeLocation,
                            TokenRequired = remoteModule.TokenRequired
                        });
                    }
                    else
                    {
                        localModule.DisplayName = remoteModule.DisplayName;
                        localModule.PublisherDisplayName = remoteModule.PublisherDisplayName;
                        localModule.OwnerName = remoteModule.OwnerName;
                        localModule.RepoName = remoteModule.RepoName;
                        localModule.RepoLink = remoteModule.RepoLink;
                        localModule.RepoType = remoteModule.RepoType;
                        localModule.Description = remoteModule.Description;
                        localModule.ExecutionName = remoteModule.ExecutionName;
                        localModule.Icon = remoteModule.Icon;
                        localModule.IconColor = remoteModule.IconColor;
                        localModule.IconImageRelativeLocation = remoteModule.IconImageRelativeLocation;
                        localModule.TokenRequired = remoteModule.TokenRequired;
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
