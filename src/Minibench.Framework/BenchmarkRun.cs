// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Minibench.Framework
{
    /// <summary>
    /// A single run of a whole suite of benchmarks.
    /// TODO: Multiple assemblies? Probably not.
    /// </summary>
    public sealed class BenchmarkRun
    {
        public static readonly XName ElementName = "benchmark";

        public BenchmarkOptions Options { get; }
        public BenchmarkEnvironment Environment { get; }

        public ImmutableList<BenchmarkTypeResults> TypeResults { get; }

        /// <summary>
        /// Convenience property to flatten all results into a single sequence.
        /// This property is lazily evaluated.
        /// </summary>
        public IEnumerable<BenchmarkResult> AllResults => TypeResults.SelectMany(tr => tr.Results);

        public ImmutableList<string> Warnings { get; }

        /// <summary>
        /// The assembly containing the benchmarks. (Note that typically the code that the benchmarks
        /// are testing will be in a different assembly.)
        /// </summary>
        public string Assembly { get; }

        /// <summary>
        /// Start time of the test run.
        /// </summary>
        public DateTimeOffset Start { get; }

        /// <summary>
        /// End time of the test run.
        /// </summary>
        public DateTimeOffset End { get; }

        /// <summary>
        /// Returns the machine from the environment, unless it's been overriden by options (in which
        /// case that takes priority).
        /// </summary>
        public string Machine => Options?.MachineOverride ?? Environment?.Machine;

        /// <summary>
        /// Convenience property to access the label associated with this run (from the options).
        /// </summary>
        public string Label => Options?.Label;

        private BenchmarkRun(Builder builder)
        {
            Assembly = builder.Assembly;
            Options = builder.Options;
            Environment = builder.Environment;
            TypeResults = builder.TypeResults.Select(x => x.Build(this)).ToImmutableList();
            Warnings = builder.Warnings.ToImmutableList();
            Start = builder.Start;
            End = builder.End;
        }

        /// <summary>
        /// Converts this run into an XML representation.
        /// </summary>
        /// <returns></returns>
        public XElement ToXElement()
        {
            return new XElement(ElementName,
                Options.ToXElement(),
                Environment.ToXElement(),
                new XAttribute("assembly", Assembly),
                new XAttribute("start", Start),
                new XAttribute("end", End),
                // TODO: Why do we have a warnings element for warnings, but no types element for types? Inconsistent.
                new XElement("warnings", Warnings.Select(warning => new XElement("warning", warning))),
                TypeResults.Select(x => x.ToXElement())
            );
        }

        /// <summary>
        /// Converts the given XML element into a run.
        /// </summary>
        public static BenchmarkRun FromXElement(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new BenchmarkRun.Builder
            {
                Assembly = (string)element.Attribute("assembly"),
                Start = (DateTimeOffset) element.Attribute("start"),
                End = (DateTimeOffset) element.Attribute("end"),
                Options = BenchmarkOptions.FromXElement(element.Element(BenchmarkOptions.ElementName)),
                Environment = BenchmarkEnvironment.FromXElement(element.Element(BenchmarkEnvironment.ElementName)),
                Warnings = element.Elements("warnings").Elements("warning").Select(x => x.Value).ToList(),
                TypeResults = element.Elements(BenchmarkTypeResults.ElementName).Select(BenchmarkTypeResults.Builder.FromXElement).ToList()
            }.Build();
        }

        public sealed class Builder
        {
            public DateTimeOffset Start { get; set; }
            public DateTimeOffset End { get; set; }
            public string Assembly { get; set; }
            public BenchmarkOptions Options { get; set; }
            public BenchmarkEnvironment Environment { get; set; }
            public List<BenchmarkTypeResults.Builder> TypeResults { get; set; } = new List<BenchmarkTypeResults.Builder>();

            public List<string> Warnings { get; set; } = new List<string>();

            public BenchmarkRun Build()
            {
                return new BenchmarkRun(this);
            }
        }
    }
}
