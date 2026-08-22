using System.Diagnostics;

namespace PowerTools.Apps.VersionInstaller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args == null || args.Length != 2)
            {
                Console.WriteLine("Invalid arguments. Please provide the paths for the extracted new version and the current version.");
                Console.ReadLine();
                return;
            }

            var tempExtractedNewVersionPath = args[0];
            var currentVersionPath = args[1];

            Console.WriteLine($"Extracted New Version Folder Path: {tempExtractedNewVersionPath}");
            Console.WriteLine($"Current Version Folder Path: {currentVersionPath}");

            Console.WriteLine($"Verify locked files in folder: {currentVersionPath}");

            var isLocked = IsFolderLocked(currentVersionPath);
            if (isLocked)
            {
                Console.WriteLine($"\nFound files are locked in folder: {currentVersionPath}");
                Console.ReadLine();
                return;
            }

            var modulesFolder = Path.Combine(currentVersionPath, "modules");
            var dataFolder = Path.Combine(currentVersionPath, "data");

            var destModulesFolder = Path.Combine(tempExtractedNewVersionPath, "modules");
            var destDataFolder = Path.Combine(tempExtractedNewVersionPath, "data");

            if(Directory.Exists(modulesFolder))
            {
                Console.WriteLine($"\nCopy {modulesFolder}");
                CopyDirectory(modulesFolder, destModulesFolder);
            }

            if(Directory.Exists(dataFolder))
            {
                Console.WriteLine($"\nCopy {dataFolder}");
                CopyDirectory(dataFolder, destDataFolder);
            }

            Console.WriteLine($"\nClean up {currentVersionPath}");
            isLocked = IsFolderLocked(currentVersionPath);
            if (isLocked)
            {
                Console.WriteLine($"\nFolder is locked for cleaning up: {currentVersionPath}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\nDelete folder: {currentVersionPath}");
            DeleteAllDirectoryContents(currentVersionPath);

            isLocked = IsFolderLocked(currentVersionPath);
            if (isLocked)
            {
                Console.WriteLine($"\nFolder is locked after cleaning up: {currentVersionPath}");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\nRestoring {currentVersionPath}");
            CopyDirectory(tempExtractedNewVersionPath, currentVersionPath);

            Console.WriteLine("\nDone!");

            var entryPoint = Path.Combine(currentVersionPath, "PowerTools.exe");
            Process.Start(entryPoint);
        }

        private static bool IsFolderLocked(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            var isLocked = false;
            var sw = Stopwatch.StartNew();

            do
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (IsFileLocked(file))
                    {
                        isLocked = true;
                        break;
                    }
                }

                if(!isLocked || sw.ElapsedMilliseconds >= 30 * 1000) // Timeout after 30 seconds
                {
                    break;
                }

            } while (true);

             
            return isLocked;
        }

        private static bool IsFileLocked(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                // Attempt to open the file with exclusive access
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // If we can open the file, it is not locked
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // If an IOException is thrown, the file is locked
                return true;
            }

            return false;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            // 1. Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            // 2. Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // 3. Copy all files in the current directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            // 4. Recursively copy all subdirectories
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string targetSubDirPath = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, targetSubDirPath, overwrite);
            }
        }

        private static void DeleteAllDirectoryContents(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            var files = Directory.GetFiles(folder);
            var dirs = Directory.GetDirectories(folder);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                {
                    Directory.Delete(dir);
                }
                else
                {
                    DeleteAllDirectoryContents(dir);
                }
            }
        }
    }
}