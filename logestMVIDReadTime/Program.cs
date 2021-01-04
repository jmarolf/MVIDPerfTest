using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace logestMVIDReadTime
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args[0]);
        }

        static void Run(string sourceDirectory)
        {
            IEnumerable<string> directories = new[] {
                Path.Combine(sourceDirectory, "eng")
            };
            var artifactsDir = Path.Combine(sourceDirectory, "artifacts/bin");
            directories = directories.Concat(Directory.EnumerateDirectories(artifactsDir, "*.UnitTests"));
            directories = directories.Concat(Directory.EnumerateDirectories(artifactsDir, "RunTests"));

            TimeSpan longestRead = TimeSpan.Zero;
            string slowestFile = "";
            var totalTime = Stopwatch.StartNew();

            foreach (var unitDirPath in directories)
            {
                foreach (var sourceFilePath in Directory.EnumerateFiles(unitDirPath, "*", SearchOption.AllDirectories))
                {
                    var currentDirName = Path.GetDirectoryName(sourceFilePath)!;
                    var currentRelativeDirectory = Path.GetRelativePath(sourceDirectory, currentDirName);
                    var fileName = Path.GetFileName(sourceFilePath);
                    if (fileName.EndsWith(".dll"))
                    {
                        //Console.WriteLine($"reading {fileName}");
                        var stopWatch = Stopwatch.StartNew();
                        if (TryGetMvid(sourceFilePath, out var mvid))
                        {
                            //Console.WriteLine("  success");
                        }
                        else
                        {
                            //Console.WriteLine("  failure");
                        }
                        var time = stopWatch.Elapsed;
                        if (time > longestRead)
                        {
                            slowestFile = fileName;
                            longestRead = time;
                        }
                    }
                }
            }
            var total = totalTime.Elapsed;


            Console.WriteLine();
            Console.WriteLine($"Slowest file to read was {slowestFile} in {longestRead}");
            Console.WriteLine($"Total time {total}");

            bool TryGetMvid(string filePath, out Guid mvid)
            {
                try
                {
                    using var stream = File.OpenRead(filePath);
                    var reader = new PEReader(stream);
                    if (!reader.HasMetadata)
                    {
                        mvid = default;
                        return false;
                    }
                    var metadataReader = reader.GetMetadataReader();
                    var mvidHandle = metadataReader.GetModuleDefinition().Mvid;
                    mvid = metadataReader.GetGuid(mvidHandle);
                    return true;
                }
                catch
                {
                    mvid = default;
                    return false;
                }
            }
        }
    }
}
