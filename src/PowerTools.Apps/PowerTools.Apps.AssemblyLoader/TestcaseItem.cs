using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTools.Apps.AssemblyLoader
{
    public class TestcaseItem
    {
        /// <summary>
        /// Name of testcase
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Class name of testcase
        /// </summary>
        public string TestClass { get; set; }

        /// <summary>
        /// Path of testcase dll
        /// </summary>
        public string DllFile { get; set; }

        public List<TestAttribute> Attributes { get; set; }

    }
}
