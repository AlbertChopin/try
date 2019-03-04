﻿using System.IO;
using System.Linq;
using System.Reactive.Concurrency;

namespace WorkspaceServer.Packaging
{
    public class RebuildablePackage : Package
    {
        public RebuildablePackage(string name = null, IPackageInitializer initializer = null, DirectoryInfo directory = null, IScheduler buildThrottleScheduler = null) 
            : base(name, initializer, directory, buildThrottleScheduler)
        {

        }

        protected override bool ShouldBuild()
        {
            var shouldBuild = base.ShouldBuild();

            if (!shouldBuild && AnalyzerResult != null)
            {
                var newAnalyzerResult = CreateAnalyzerResult();
                if (LastSuccessfulBuildTime != null && new FileInfo(newAnalyzerResult.ProjectFilePath).LastWriteTimeUtc > LastSuccessfulBuildTime)
                {
                    return true;
                }

                var analyzerInputs = AnalyzerResult.GetCompileInputs();
                var newInputs = newAnalyzerResult.GetCompileInputs();

                if (!newInputs.SequenceEqual(analyzerInputs))
                {
                    return true;
                }

                var lastWriteTimes = analyzerInputs.Select(f => new FileInfo(f)).Where(fi => fi.LastWriteTimeUtc > LastSuccessfulBuildTime).ToArray();

                if (lastWriteTimes.Any())
                {
                    return true;
                }
            }

            return shouldBuild;
        }
    }

 
}
