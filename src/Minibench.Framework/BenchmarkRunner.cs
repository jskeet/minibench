// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minibench.Framework
{
    public sealed class BenchmarkRunner
    {
        private const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // Most of the options are relevant for the runner, so it's not worth splitting them out
        // into *just* the bits the runner is interested in.
        private readonly BenchmarkOptions options;
        private readonly BenchmarkResultHandler originalResultHandler;
        // The result builder to funnel all the actual results through, so we can extract the run at the end.
        private BenchmarkResultHandler resultHandler;

        /// <summary>
        /// Convenience method to parse options from the command line, construct an appropriate console result
        /// handler, and save results to an XML file if specified. Returns the results of the run, or null if
        /// the options couldn't be parsed. (An error message will already have been displayed.)
        /// </summary>
        public static BenchmarkRun RunFromCommandLine(Assembly assembly)
        {
            var options = BenchmarkOptions.FromCommandLine();
            if (options == null)
            {
                return null;
            }
            var handler = new ConsoleResultHandler(options.DisplayRawData);
            var runner = new BenchmarkRunner(options, handler);
            var run = runner.RunTests(assembly);
            if (options.XmlFile != null)
            {
                run.ToXElement().Save(options.XmlFile);
            }
            return run;
        }

        public BenchmarkRunner(BenchmarkOptions options, BenchmarkResultHandler originalResultHandler)
        {
            this.options = options;
            this.originalResultHandler = originalResultHandler;
        }

        // TODO: Move some of this to a new type, I think...

        public BenchmarkRun RunTests(Assembly assembly)
        {
            var runBuilder = new BenchmarkRunBuildingHandler();
            resultHandler = new CompositeResultHandler(new[] { runBuilder, originalResultHandler });

            resultHandler.HandleStartRun(assembly, BenchmarkEnvironment.CurrentSystem, options);
            var types = assembly.GetTypes()
                                .OrderBy(type => type.FullName)
                                .Where(type => type.GetMethods(AllInstance)
                                .Any(IsBenchmark));

            foreach (Type type in types)
            {
                if (options.TypeFilter != null && type.Name != options.TypeFilter)
                {
                    continue;
                }

                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    resultHandler.HandleWarning($"Ignoring {type.Name}: no public parameterless constructor");
                    continue;
                }
                resultHandler.HandleStartType(type);
                object instance = ctor.Invoke(null);
                RunTestMethods(type, instance);
                resultHandler.HandleEndType();
            }
            resultHandler.HandleEndRun();
            return runBuilder.BuildRun();
        }

        private void RunTestMethods(Type type, object instance)
        {
            foreach (var method in type.GetMethods(AllInstance).Where(IsBenchmark))
            {
                var categories = GetCategories(method);
                if (options.IncludedCategories != null && !categories.Overlaps(options.IncludedCategories))
                {
                    continue;
                }
                if (options.ExcludedCategories != null && categories.Overlaps(options.ExcludedCategories))
                {
                    continue;
                }

                if (options.MethodFilter != null && !MethodMatchesFilter(method, options.MethodFilter))
                {
                    continue;
                }

                if (method.GetParameters().Length != 0)
                {
                    resultHandler.HandleWarning(string.Format("Ignoring {0}: method has parameters", method.Name));
                    continue;
                }
                RunBenchmark(method, instance, options);
            }
        }

        private bool MethodMatchesFilter(MethodInfo method, string methodFilter)
        {
            if (!methodFilter.EndsWith("*"))
            {
                return method.Name == methodFilter;
            }
            return method.Name.StartsWith(methodFilter.Substring(0, methodFilter.Length - 1));
        }

        private static HashSet<string> GetCategories(MethodInfo method)
        {
            var categories = method.GetCustomAttributes(typeof(CategoryAttribute), false)
                                   .Concat(method.DeclaringType.GetCustomAttributes(typeof(CategoryAttribute), false))
                                   .Cast<CategoryAttribute>()
                                   .Select(c => c.Category);
            return new HashSet<string>(categories);
        }

        private void RunBenchmark(MethodInfo method, object instance, BenchmarkOptions options)
        {
            var action = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);

            if (options.DryRunOnly)
            {
                action();
                // Fake a result, partly so that dry runs can be used to test result handling.
                resultHandler.HandleResult(method.Name, 1, TimeSpan.FromTicks(1));
                return;
            }

            // Start small, double until we've hit our warm-up time
            int iterations = 100;
            while (true)
            {
                TimeSpan duration = RunTest(action, iterations, options.Timer);
                if (duration >= options.WarmUpTime)
                {
                    // Scale up the iterations to work out the full test time
                    double scale = ((double)options.TestTime.Ticks) / duration.Ticks;
                    double scaledIterations = scale * iterations;
                    // Make sure we never end up overflowing...
                    iterations = (int)Math.Min(scaledIterations, int.MaxValue - 1);
                    break;
                }
                // Make sure we don't end up overflowing due to doubling...
                if (iterations >= int.MaxValue / 2)
                {
                    break;
                }
                iterations *= 2;
            }
            TimeSpan testTimeSpan = RunTest(action, iterations, options.Timer);
            if (testTimeSpan == TimeSpan.Zero)
            {
                resultHandler.HandleWarning($"Test {method.Name} had zero TimeSpan; no useful result.");
            }
            else
            {
                resultHandler.HandleResult(method.Name, iterations, testTimeSpan);
            }
        }

        private static TimeSpan RunTest(Action action, int iterations, IBenchTimer timer)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            timer.Reset();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            return timer.ElapsedTime;
        }

        private static bool IsBenchmark(MethodInfo method)
        {
            return method.IsDefined(typeof(BenchmarkAttribute), false);
        }
    }
}
