using System.Collections.Generic;

namespace PowerTools.Core.Models
{
    public class Repository<T>
    {
        public ToolModule SelectedModule { get; set; }
        public List<T> ModuleList { get; set; }

        public Repository()
        {
            SelectedModule = null;
            ModuleList = new List<T>();
        }
    }
}
