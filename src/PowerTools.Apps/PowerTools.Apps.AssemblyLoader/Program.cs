using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace PowerTools.Apps.AssemblyLoader
{
    public class Program
    {
        public class IsolatedAssemblyLoadContext : AssemblyLoadContext
        {
            private readonly string _assemblyDirectory;

            public IsolatedAssemblyLoadContext(string assemblyDirectory, bool isCollectible = true)
                : base(isCollectible)
            {
                _assemblyDirectory = assemblyDirectory;
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                // Try to load from the assembly directory first
                var assemblyPath = Path.Combine(_assemblyDirectory, assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    Console.WriteLine($"Loading dependency from local directory: {assemblyName.Name}");
                    return LoadFromAssemblyPath(assemblyPath);
                }

                // Let the default context handle system assemblies
                return null;
            }
        }

        public static void Main(string[] args)
        {
            var jsonPath = @"C:\Users\HP\Downloads\archive\test-config.json";
            var jsonContent = File.ReadAllText(jsonPath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);



            if (args.Length < 2)
            {
                return;
            }

            var assemblyFolderPath = args[0];
            var dll = args[1];
            var outputFilePath = args[2];
            var assemblyPath = Path.Combine(assemblyFolderPath, dll);

            if (!File.Exists(assemblyPath))
            {
                return;
            }

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var listTestcases = new List<TestcaseItem>();
            /*
            var isolatedContext = new IsolatedAssemblyLoadContext(assemblyFolderPath, true);
            var assembly = isolatedContext.LoadFromAssemblyPath(assemblyPath);
            */
            var assembly = Assembly.LoadFrom(assemblyPath);

            if (assembly == null)
            {
                return;
            }

            foreach (var type in assembly.GetTypes())
            {
                /* Get a CustomAttributes of a type then see
                 * If it contains CustomAttribute "NUnit.Framework.TestFixtureAttribute" -> This is a test class
                 */
                var attributes = System.Attribute.GetCustomAttributes(type);
                var isTestFixtureAttribute = attributes?.Length > 0 && attributes.Any(p => p.ToString() == "NUnit.Framework.TestFixtureAttribute");

                if (isTestFixtureAttribute)
                {
                    var testClass = type.FullName;

                    /* Get all methods of a test class
                     * If a method contains CustomAttribute "NUnit.Framework.TestAttribute" -> This is a testcase
                     */
                    var allMethods = type.GetMethods();
                    foreach (var method in allMethods)
                    {
                        var methodCustomAttributes = method.CustomAttributes;
                        var isTestcase = methodCustomAttributes.Any(p => p.ToString() == "[NUnit.Framework.TestAttribute()]");

                        if (isTestcase)
                        {
                            var testAttributes = new List<TestAttribute>();
                            var remainingAttributes = methodCustomAttributes.Where(p => p.ToString() != "[NUnit.Framework.TestAttribute()]");
                            foreach (var remainingAttribute in remainingAttributes)
                            {
                                var attributeName = remainingAttribute.AttributeType.Name.Replace("Attribute", string.Empty);
                                var attributeValue = remainingAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();

                                if (attributeName == "Reviewed" && attributeValue == "0")
                                {
                                    attributeValue = "Pass";
                                }
                                else if (attributeName == "Reviewed" && attributeValue == "1")
                                {
                                    attributeValue = "Failed";
                                }
                                else if (attributeName == "Reviewed" && attributeValue == "2")
                                {
                                    attributeValue = "Rejects";
                                }

                                testAttributes.Add(new TestAttribute
                                {
                                    Name = attributeName,
                                    Value = attributeValue
                                });
                            }

                            listTestcases.Add(new TestcaseItem
                            {
                                Name = method.Name,
                                TestClass = testClass,
                                DllFile = dll,
                                Attributes = testAttributes
                            });
                        }
                    }
                }
            }

            Console.WriteLine("Loaded Assembly and test cases "+ listTestcases.Count);
            File.WriteAllText(outputFilePath, System.Text.Json.JsonSerializer.Serialize(listTestcases));
        }
    }
}