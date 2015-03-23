// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Reflection;

namespace Minibench.Framework
{
    /// <summary>
    /// Result handler which just writes results to the console.
    /// </summary>
    public sealed class ConsoleResultHandler : BenchmarkResultHandler
    {
        private const string LongFormatString = "  {0}: {1:N0} iterations/second; ({2:N0} iterations in {3:N0} ticks; {4:N0} nanoseconds/iteration)";
        private const string ShortFormatString = "  {0}: {1:N0} iterations/second ({4:N0} nanoseconds/iteration)";

        private readonly string formatString;

        public ConsoleResultHandler(bool rawResults)
        {
            formatString = rawResults ? LongFormatString : ShortFormatString;
        }

        public override void HandleStartRun(Assembly assembly, BenchmarkEnvironment environment, BenchmarkOptions options)
        {
            Console.WriteLine("Environment: CLR {0} on {1} ({2})", environment.RuntimeVersion, environment.OperatingSystem,
                environment.Is64BitProcess ? "64 bit" : "32 bit");
            if (options.Label != null)
            {
                Console.WriteLine("Run label: {0}", options.Label);
            }
        }

        public override void HandleStartType(Type type)
        {
            Console.WriteLine("Running benchmarks in {0}", GetTypeDisplayName(type));
        }

        public override void HandleResult(string method, int iterations, TimeSpan duration)
        {
            // TODO: Remove the duplication between here and BenchmarkResult.
            long callsPerSecond = iterations * TimeSpan.TicksPerSecond / duration.Ticks;
            long nanosecondsPerCall = duration.Ticks * BenchmarkResult.TicksPerNanosecond / iterations;
            Console.WriteLine(formatString, method, callsPerSecond,
                iterations, duration.Ticks, nanosecondsPerCall);
        }

        // FIXME: Probably rubbish.
        private static string GetTypeDisplayName(Type type)
        {
            return type.FullName.Replace("Minibench.", "");
        }
    }
}
