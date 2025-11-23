using PowerTools.Core.SharedServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PowerTools.Core.Models
{
    public abstract class Repository<T>
    {
        public RepositoryInformation<T> RepositoryInformation { get; protected set; }

        public List<T> ModuleList
        {
            get
            {
                if (RepositoryInformation == null)
                {
                    return new List<T>();
                }

                return RepositoryInformation.ModuleList ?? new List<T>();
            }
        }

        public abstract string GetRepositoryPath();
        public abstract void Store();

        protected virtual void BeforeLoad(string repositoryPath)
        {
            // TODO: do nothing
        }

        protected virtual RepositoryInformation<T> AfterLoad(RepositoryInformation<T> repositoryInformation)
        {
            return repositoryInformation;
        }

        public virtual List<T> Load(bool useCache = true)
        {
            if (useCache)
            {
                if (RepositoryInformation == null)
                {
                    RepositoryInformation = Format(null);
                }

                return RepositoryInformation.ModuleList;
            }

            RepositoryInformation<T> repositoryInformation;
            var repoPath = GetRepositoryPath();

            BeforeLoad(repoPath);

            try
            {
                if (!File.Exists(repoPath))
                    throw new FileNotFoundException($"Not found repository file '{repoPath}'");

                var jsonContent = File.ReadAllText(repoPath);
                repositoryInformation = JsonSerializer.Deserialize<RepositoryInformation<T>>(jsonContent);
                repositoryInformation = Format(repositoryInformation);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error($"Failed to local repository local '{repoPath}'", e);
                repositoryInformation = Format(null);
            }

            RepositoryInformation = AfterLoad(repositoryInformation);

            return RepositoryInformation.ModuleList;
        }

        private RepositoryInformation<T> Format(RepositoryInformation<T> repoInformation)
        {
            if (repoInformation == null)
            {
                return new RepositoryInformation<T>
                {
                    ModuleList = new List<T>()
                };
            }

            if (repoInformation.ModuleList == null)
            {
                repoInformation.ModuleList = new List<T>();
            }

            return repoInformation;
        }
    }
}
