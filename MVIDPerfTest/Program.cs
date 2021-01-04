using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace MVIDPerfTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .AddJob(
                    Job.Default.WithToolchain(
                        CsProjCoreToolchain.From(
                            new NetCoreAppSettings(
                                targetFrameworkMoniker: "net5.0-windows",  // the key to make it work
                                runtimeFrameworkVersion: null,
                                name: "5.0")))
                                .AsDefault());

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }

    public class MVIDBenchmark
    {
        private static string TestPath = @"C:\source\dotnet\roslyn\artifacts\bin\Roslyn.VisualStudio.Next.UnitTests\Debug\net472\Castle.Core.dll";

        [Benchmark(Description = "Current")]
        public void MVIDBenchmarkCurrent()
        {
            if (MVID.TryGetMvidCurrent(TestPath, out var mvid))
            {
            }
        }

        [Benchmark(Description = "PrefetchMetadata")]
        public void MVIDBenchmarkPrefetchMetadata()
        {
            if (MVID.TryGetMvidPrefetchMetadata(TestPath, out var mvid))
            {
            }
        }

    }


    static class MVID
    {
        public static bool TryGetMvidPrefetchMetadata(string filePath, out Guid mvid)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var reader = new PEReader(stream, PEStreamOptions.PrefetchMetadata);
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

        public static bool TryGetMvidCurrent(string filePath, out Guid mvid)
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
