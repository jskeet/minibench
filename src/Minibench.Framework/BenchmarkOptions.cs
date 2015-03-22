// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Minibench.Framework
{
    /// <summary>
    /// Encapsulates all the options for benchmarking, such as
    /// the approximate length of each test, the timer to use
    /// and so on.
    /// </summary>
    public sealed class BenchmarkOptions
    {
        public static readonly XName ElementName = "options";

        private BenchmarkOptions(Builder builder)
        {
            DryRunOnly = builder.DryRunOnly;
            TypeFilter = builder.TypeFilter;
            MethodFilter = builder.MethodFilter;
            WarmUpTime = TimeSpan.FromSeconds(builder.WarmUpTimeSeconds);
            TestTime = TimeSpan.FromSeconds(builder.TestTimeSeconds);
            Timer = new WallTimer();
            DisplayRawData = builder.DisplayRawData;
            XmlFile = builder.XmlFile;
            Label = builder.Label;
            // TODO: Rename to "Profile"? This is somewhat ugly.
            MachineOverride = builder.MachineOverride;
            IncludedCategories = builder.IncludedCategories?.Split(',').ToImmutableList();
            ExcludedCategories = builder.ExcludedCategories?.Split(',').ToImmutableList();
        }

        public TimeSpan WarmUpTime { get; }
        public TimeSpan TestTime { get; }
        public IBenchTimer Timer { get; }
        public string TypeFilter { get; }
        public string MethodFilter { get; }
        public bool DisplayRawData { get; }
        public string XmlFile { get; }
        public bool DryRunOnly { get; }
        public string Label { get; }
        public string MachineOverride { get; }
        public ImmutableList<string> IncludedCategories { get; }
        public ImmutableList<string> ExcludedCategories { get; }

        public XElement ToXElement()
        {
            // Output-only options are not recorded (DisplayRawData, XmlFile). Ditto IBenchTimer (which can't actually be specified at the moment).
            return new XElement(ElementName,
                new XAttribute("warmup-time", WarmUpTime),
                new XAttribute("test-time", TestTime),
                Label == null ? null :  new XAttribute("label", Label),
                TypeFilter == null ? null : new XAttribute("type-filter", TypeFilter),
                MethodFilter == null ? null : new XAttribute("method-filter", MethodFilter),
                IncludedCategories == null ? null : new XAttribute("included-categories", string.Join(",", IncludedCategories)),
                ExcludedCategories == null ? null : new XAttribute("excluded-categories", string.Join(",", ExcludedCategories)),
                !DryRunOnly ? null : new XAttribute("dry-run", DryRunOnly),
                MachineOverride == null ? null : new XAttribute("machine-override", MachineOverride)
            );
        }

        public static BenchmarkOptions FromXElement(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new Builder
            {
                WarmUpTimeSeconds = (int) ((TimeSpan) element.Attribute("warmup-time")).TotalSeconds,
                TestTimeSeconds = (int) ((TimeSpan)element.Attribute("test-time")).TotalSeconds,
                Label = (string) element.Attribute("label"),
                TypeFilter = (string) element.Attribute("type-filter"),
                MethodFilter = (string) element.Attribute("method-filter"),
                IncludedCategories = (string) element.Attribute("included-categories"),
                ExcludedCategories = (string) element.Attribute("excluded-categories"),
                DryRunOnly = (bool?) element.Attribute("dry-run") ?? false,
                MachineOverride = (string) element.Attribute("machine-override")
            }.Build();
        }

        public static BenchmarkOptions FromCommandLine()
        {
            return FromArguments(Environment.GetCommandLineArgs());
        }

        public static BenchmarkOptions FromArguments(string[] args)
        {
            Builder options = new Builder
            {
                WarmUpTimeSeconds = 1,
                TestTimeSeconds = 10
            };
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options))
            {
                return null;
            }
            return options.Build();
        }

        public sealed class Builder
        {
            [Option("w", "warmup", HelpText = "TimeSpan of warm-up time per test, in seconds. Default=1")]
            public int WarmUpTimeSeconds { get; set; }
            [Option("d", "duration", HelpText = "Targeted per-test TimeSpan, in seconds. Default=10")]
            public int TestTimeSeconds { get; set; }
            [Option("t", "type", HelpText = "Type filter")]
            public string TypeFilter { get; set; }
            [Option("m", "method", HelpText = "Method filter (use trailing * for wildcard)")]
            public string MethodFilter { get; set; }
            [Option("r", "raw", HelpText = "Display the raw data")]
            public bool DisplayRawData { get; set; }
            [Option("x", "xml", HelpText = "Write to the given XML file as well as the console")]
            public string XmlFile { get; set; }
            [Option("!", "dry", HelpText = "Dry run mode: run tests once each, with no timing, just to validate")]
            public bool DryRunOnly { get; set; }
            [Option("l", "label", HelpText = "Test run label")]
            public string Label { get; set; }
            [Option("o", "machine", HelpText = "Machine name override")]
            public string MachineOverride { get; set; }
            [Option(null, "included-categories", HelpText = "Included categories, comma-separated")]
            public string IncludedCategories { get; set; }
            [Option(null, "excluded-categories", HelpText = "Excluded categories, comma-separated")]
            public string ExcludedCategories { get; set; }

            [HelpOption("?", "help", HelpText = "Display this help screen.")]
            public string GetUsage()
            {
                var help = new HelpText(new HeadingInfo("Minibench"))
                {
                    AdditionalNewLineAfterOption = true,
                };
                help.AddOptions(this);
                return help;
            }

            public BenchmarkOptions Build()
            {
                return new BenchmarkOptions(this);
            }
        }
    }
}