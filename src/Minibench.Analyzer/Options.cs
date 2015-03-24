// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using CommandLine;
using CommandLine.Text;

namespace Minibench.Analyzer
{
    /// <summary>
    /// Encapsulates all the options for reporting.
    /// </summary>
    internal class Options
    {
        [Option("m", "machine", Required = false, HelpText = "Machine to retrieve results for")]
        public string Machine { get; set; }

        [Option("d", "directory", Required = true, HelpText = "Directory to load benchmarks from")]
        public string Directory { get; set; }

        [Option("r", "regression-threshold", DefaultValue = 130,
                HelpText = "Regression trigger threshold: the percentage of the previous time that the new time must exceed to report a regression")]
        public int RegressionThreshold { get; set; }

        [Option("i", "improvement-threshold", DefaultValue = 80,
                HelpText = "Improvements trigger threshold: the percentage of the previous time that the new time must fall below to report an improvement")]
        public int ImprovementThreshold { get; set; }

        [Option("n", "count", DefaultValue = 3,
                HelpText = "Count to use when smoothing; improvements and regressions are reported based on sums of times")]
        public int SmoothingCount { get; set; }
        
        [HelpOption("?", "help", HelpText = "Display this help screen.")]
        public string GetUsage()
        {
            var help = new HelpText(new HeadingInfo("Minibench.Analyzer"));
            help.AddOptions(this);
            return help;
        }
    }
}
