using System.Collections.Generic;

namespace PowerTools.Core.Models
{
    public class RepositoryInformation<T>
    {
        private string _version;
        public string Version
        {
            get =>  _version;
            set
            {
                _version = value;
                if (string.IsNullOrEmpty(_version) || string.IsNullOrWhiteSpace(_version))
                {
                    _version = "1.0";
                }
            }
        }

        public List<T> ModuleList { get; set; }
    }
}
