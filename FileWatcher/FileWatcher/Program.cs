using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileWatcher
{
    class Program
    {
        readonly static string sourceLocation = @"..\..\..\..\..\mobirise-template";
        readonly static string destinationSourceLocation = @"..\..\..\..\..\mobirise-angular-integration\src";
        readonly static string destinationProjectLocation = @"..\..\..\..\..\mobirise-angular-integration";
        readonly static string mainContainerFile = @"..\..\..\..\..\mobirise-angular-integration\src\app\main-container\main-container.component.html";
        
        static void Main(string[] args)
        {
            Console.WriteLine("started FileWatcher");
            RunProjectInitial();
            Console.WriteLine("Filewatcher is running properly");
            Console.WriteLine("Press any key to stop fileWatcher");
            FileSystemWatcher watcher = InitialiseFileWatcher();
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            Console.ReadKey();
        }

        private static void RunProjectInitial()
        {
            CopyAssetDirectory();
            CreateComponentForEverySection();
            RemoveAllDeletedComponents();
            SetAngularIndexFile();
            ServeApplication();
        }

        private static FileSystemWatcher InitialiseFileWatcher()
        {
            var path = sourceLocation;
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "index.html";
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            CopyAssetDirectory();
            CreateComponentForEverySection();
            RemoveAllDeletedComponents();
            SetAngularIndexFile();
        }

        private static void CopyAssetDirectory() {
            var diSource = new DirectoryInfo(sourceLocation + @"\assets");
            var diTarget = new DirectoryInfo(destinationSourceLocation + @"\assets");
            CopyDirectoryRecursive(diSource, diTarget);
        }

        private static void CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectoryRecursive(diSourceSubDir, nextTargetSubDir);
            }
        }

        private static void CreateComponentForEverySection()
        {
            if (WaitForFile(sourceLocation + @"\index.html"))
            {
                var sections = GetSectionsFromHTML();
                ClearMainContainer();
                if (sections != null)
                {
                    foreach (var section in sections)
                    {
                        string componentCode = section.OuterHtml;
                        string componentName = new string(section.Id.Where(char.IsLetter).ToArray()).ToLower();
                        if (componentName == "")
                        {
                            Console.WriteLine("---------------------------------");
                            Console.WriteLine("Thats a standart message you dont have to worry");
                            Console.WriteLine("Could not create the following section because it misses an id");
                            Console.WriteLine("***");
                            Console.WriteLine(section.OuterHtml);
                            Console.WriteLine("***");
                            Console.WriteLine("---------------------------------");
                        }
                        else
                        {
                            CreateComponentFromCommandLine(componentName);
                            CopySectionCodeToComponentTemplate(componentName, componentCode);
                            AddToMainContainer(componentName);
                        }
                    }
                }
            }
            
            

        }

        private static string GetComponentNameOfSection(HtmlNode section) 
        {
            return new string(section.Id.Where(char.IsLetter).ToArray()).ToLower();
        }

        private static HtmlNodeCollection GetSectionsFromHTML()
        {
            string text = File.ReadAllText(sourceLocation + @"\index.html");
            var document = new HtmlDocument();
            document.LoadHtml(text);
            HtmlNodeCollection sections = document.DocumentNode.SelectNodes("//html/body/section");

            return sections;
        }

        private static void CreateComponentFromCommandLine(string componentName)
        {
            if (!ComponentDoesExist(componentName))
            {
                var processStartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = destinationProjectLocation,
                    FileName = "cmd.exe",
                    UseShellExecute = true,
                    Arguments = "/c ng generate component mobirise-components/" + componentName
                };
                Process.Start(processStartInfo).WaitForExit();
            }
        }

        private static bool ComponentDoesExist(string componentName) 
        {
            return Directory.Exists(destinationSourceLocation + @"\app\mobirise-components\" + componentName);
        }

        private static void CopySectionCodeToComponentTemplate(string componentName, string componentCode)
        {
            string directory = destinationSourceLocation + @"\app\mobirise-components\" + componentName + @"\" + componentName + ".component.html";
            File.WriteAllText(directory, componentCode);
        }

        private static void ClearMainContainer()
        {
            string containerLocation = mainContainerFile;
            File.WriteAllText(containerLocation, "");
        }

        private static void AddToMainContainer(string componentName)
        {
            string containerDirectory = mainContainerFile;
            string text = "<app-" + componentName + "></app-" + componentName + ">\n";
            File.AppendAllText(containerDirectory, text);
        }

        private static void RemoveAllDeletedComponents()
        {
            var sourceSections = GetSectionsFromHTML();
            var existingAngularComponents = GetAllAngularComponents();
            foreach (var angularComponent in existingAngularComponents)
            {
                bool doesExistInSource = false;
                foreach (var sourceComponent in sourceSections)
                {
                    string sourceComponentName = GetComponentNameOfSection(sourceComponent);
                    if (angularComponent == sourceComponentName)
                    {
                        doesExistInSource = true;
                    }
                }
                if (!doesExistInSource && angularComponent != "main-container")
                {
                    RemoveAngularComponent(angularComponent);
                }
            }
        }

        private static string[] GetAllAngularComponents()
        {
            return Directory.GetDirectories(destinationSourceLocation + @"\app\mobirise-components")
                            .Select(Path.GetFileName)
                            .ToArray();
        }

        private static void RemoveAngularComponent(string componentName)
        {
            RemoveComponentFromAppModule(componentName);
            DeleteComponentFolder(componentName);
        }

        private static void RemoveComponentFromAppModule(string componentName)
        {
            var lines = File.ReadAllLines(destinationSourceLocation + @"\app\app.module.ts").ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines.ElementAt(i).ToLower().Contains(componentName+"component"))
                {
                    lines.RemoveAt(i);
                }
            }
            File.WriteAllLines(destinationSourceLocation + @"\app\app.module.ts", lines);
        }

        private static void DeleteComponentFolder(string componentName)
        {
            Directory.Delete(destinationSourceLocation + @"\app\mobirise-components\" + componentName, true);
        }

        private static void SetAngularIndexFile()
        {
            string sourceText = File.ReadAllText(sourceLocation + @"\index.html");
            var document = new HtmlDocument();
            document.LoadHtml(sourceText);
            HtmlNodeCollection scriptCollection = document.DocumentNode.SelectNodes("//html/body/script");
            HtmlNodeCollection headCollection = document.DocumentNode.SelectNodes("//html/head");
            string destination = destinationSourceLocation + @"\index.html";
            WriteToHtml(destination, scriptCollection, headCollection);
        }

        private static void WriteToHtml(string destination, HtmlNodeCollection sourceHeadCollection, HtmlNodeCollection sourceScriptCollection )
        {
            if (sourceHeadCollection != null && sourceScriptCollection != null)
            {
                File.WriteAllText(destination, "");
                File.AppendAllText(destination, "<!DOCTYPE html>" + "\n");
                File.AppendAllText(destination, "<html>" + "\n");
                File.AppendAllText(destination, "<head>" + "\n");
                File.AppendAllText(destination, "<base href='/'>" + "\n");

                foreach (var item in sourceHeadCollection)
                {
                    File.AppendAllText(destination, item.InnerHtml + "\n");
                }

                File.AppendAllText(destination, "</head>" + "\n");
                File.AppendAllText(destination, "<body>" + "\n");
                File.AppendAllText(destination, "<app-root></app-root>" + "\n");

                foreach (var item in sourceScriptCollection)
                {
                    File.AppendAllText(destination, item.OuterHtml + "\n");
                }

                File.AppendAllText(destination, "</body>" + "\n");
                File.AppendAllText(destination, "</html>" + "\n");
            }
           
        }

        private static void ServeApplication()
        {
            var processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = destinationProjectLocation,
                FileName = "cmd.exe",
                UseShellExecute = true,
                Arguments = "/c ng serve"
            };
            Process.Start(processStartInfo);
        }

        private static bool WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    // Attempt to open the file exclusively.
                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        // If we got this far the file is ready
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (numTries > 10)
                    {
                        Console.WriteLine(
                            "WaitForFile {0} giving up after 10 tries",
                            fullPath);
                        return false;
                    }

                    // Wait for the lock to be released
                    System.Threading.Thread.Sleep(500);
                }
            }

           Console.WriteLine("WaitForFile {0} returning true after {1} tries",
                fullPath, numTries);
            return true;
        }
    }
}
