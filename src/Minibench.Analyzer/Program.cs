﻿// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using CommandLine;
using Minibench.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Minibench.Analyzer
{
    class Program
    {
        static int Main(string[] args)
        {
            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
            {
                return 1;
            }
            var benchmarks = LoadBenchmarks(options.Directory, options.Machine);
            foreach (var group in benchmarks.GroupBy(file => file.Machine))
            {
                AnalyzeResults(group, options);
            }
            return 0;
        }

        static IEnumerable<BenchmarkRun> LoadBenchmarks(string directory, string machine)
        {
            return Directory.GetFiles(directory, "*.xml")
                            .Select(file => BenchmarkRun.FromXElement(XElement.Load(file)))
                            .Where(run => machine == null || run.Machine == machine)
                            .OrderBy(run => run.Start);
        }

        static void AnalyzeResults(IEnumerable<BenchmarkRun> runs, Options options)
        {
            Console.WriteLine("Results for {0}", runs.First().Environment.Machine);
            var methodResults = from run in runs
                                from result in run.TypeResults.SelectMany(r => r.Results)
                                group new { run, result } by result.FullMethod;

            foreach (var resultSet in methodResults)
            {
                var method = resultSet.Key;
                var results = Smooth(resultSet,
                                     pairs => new { StartTime = pairs.First().run.Start,
                                                    Label = pairs.First().run.Options.Label,
                                                    Average = pairs.Average(x => x.result.NanosecondsPerCall) },
                                     options.SmoothingCount);
                var resultPairs = results.Zip(results.Skip(1), (previous, current) => new { previous, current });
                foreach (var pair in resultPairs)
                {
                    var currentAverage = (int) pair.current.Average;
                    var previousAverage = (int)pair.previous.Average;
                    var start = pair.current.StartTime;
                    var label = pair.current.Label ?? "None";
                    if (currentAverage * 100 < previousAverage * options.ImprovementThreshold)
                    {
                        Console.WriteLine("{0:yyyy-MM-dd} ({1}): Improvement\r\n{2}: {3} to {4} ns per call",
                            start, label, method, previousAverage, currentAverage);
                    }
                    if (pair.current.Average * 100 > pair.previous.Average * options.RegressionThreshold)
                    {
                        Console.WriteLine("{0:yyyy-MM-dd} ({1}): Regression\r\n{2} from {3} to {4} ns per call",
                            start, label, method, previousAverage, currentAverage);
                    }
                }
            }
        }

        static IEnumerable<TResult> Smooth<TItem, TResult>(IEnumerable<TItem> source,
            Func<IEnumerable<TItem>, TResult> smoothingFunction, int count)
        {
            Queue<List<TItem>> queue = new Queue<List<TItem>>();
            foreach (var item in source)
            {
                if (queue.Count < count)
                {
                    queue.Enqueue(new List<TItem>());
                }
                foreach (var list in queue)
                {
                    list.Add(item);
                }
                if (queue.Count == count)
                {
                    yield return smoothingFunction(queue.Dequeue());
                    queue.Enqueue(new List<TItem>());
                }
            }
        }
    }
}
