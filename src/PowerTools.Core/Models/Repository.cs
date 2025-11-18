using System;
using System.Collections.Generic;

namespace PowerTools.Core.Models
{
    public class Repository<T>
    {
        public string SelectedModuleName { get; set; }

        public List<T> ModuleList { get; set; }

        public Repository()
        {
            ModuleList = new List<T>();
        }
    }
}
