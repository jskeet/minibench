// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Reflection;

namespace Minibench.Framework
{
    /// <summary>
    /// A result handler which builds the complete immutable benchmark run.
    /// TODO: Better name, and state validation.
    /// </summary>
    public sealed class BenchmarkRunBuildingHandler : BenchmarkResultHandler
    {
        private BenchmarkRun.Builder runBuilder;
        private BenchmarkTypeResults.Builder typeBuilder;

        public override void HandleStartRun(Assembly assembly, BenchmarkEnvironment environment, BenchmarkOptions options)
        {
            runBuilder = new BenchmarkRun.Builder {
                Assembly = assembly.GetName().Name,
                Environment = environment,
                Options = options,
                Start = DateTimeOffset.UtcNow
            };
        }

        public override void HandleEndRun()
        {
            runBuilder.End = DateTimeOffset.UtcNow;
        }

        public override void HandleStartType(Type type)
        {
            typeBuilder = new BenchmarkTypeResults.Builder {Type = type.Name, Namespace = type.Namespace};
            runBuilder.TypeResults.Add(typeBuilder);
        }

        public override void HandleEndType()
        {
            typeBuilder = null;
        }

        public override void HandleResult(string method, int iterations, TimeSpan duration)
        {
            typeBuilder.Results.Add(new BenchmarkResult.Builder {Method = method, Iterations = iterations, Duration = duration});
        }

        public override void HandleWarning(string text)
        {
            // It's either a warning for a type or for the whole run, depending on context.
            (typeBuilder == null ? runBuilder.Warnings : typeBuilder.Warnings).Add(text);
        }

        public BenchmarkRun BuildRun()
        {
            return runBuilder.Build();
        }
    }
}
