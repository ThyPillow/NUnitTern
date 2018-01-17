using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;

namespace NUnitTern
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Arguments of the program should be a list of csproj to migrate");
            }

            foreach (var projectFilePath in args.Distinct())
            {
                var fileInfo = new FileInfo(projectFilePath);
                if (!fileInfo.Exists)
                {
                    Console.WriteLine($"Non existant file '{projectFilePath}'");
                    return;
                }
                if (fileInfo.Extension != ".csproj")
                {
                    Console.WriteLine($"File {fileInfo.Name} is not a csproj");
                    return;
                }
                Console.WriteLine($"Starting migration of {fileInfo.Name}");

                var workspace = MSBuildWorkspace.Create();
                var current = 1;
                var breakingChanges = BreakingChangeRepository.BreakingChanges;

                foreach (var breakingChange in breakingChanges)
                {
                    Console.WriteLine($"{current++}/{breakingChanges.Count} - {breakingChange.EquivalenceKey} - Starting migration");
                    new Tern(workspace, projectFilePath).Migrate(breakingChange);
                }

                Console.WriteLine();
                Console.WriteLine("End of diagnostics and fixes. Enter to exit");
                Console.ReadLine();
            }
        }
    }
}
