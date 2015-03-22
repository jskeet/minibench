// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Minibench.Framework
{
    /// <summary>
    /// A set of benchmark results from a single run, for a single type.
    /// </summary>
    public sealed class BenchmarkTypeResults
    {
        public static XName ElementName = "type";

        public BenchmarkRun Run { get; }
        public string Namespace { get; }
        public string Type { get; }

        /// <summary>
        /// The type name including namespace.
        /// </summary>
        public string FullType => $"{Namespace}.{Type}";

        public ImmutableList<BenchmarkResult> Results { get; }

        public ImmutableList<string> Warnings { get; }

        private BenchmarkTypeResults(Builder builder, BenchmarkRun run)
        {
            this.Run = run;
            Results = builder.Results.Select(x => x.Build(this)).ToImmutableList();
            Namespace = builder.Namespace;
            Type = builder.Type;
            Warnings = builder.Warnings.ToImmutableList();
        }

        public XElement ToXElement()
        {
            return new XElement(ElementName,
                new XAttribute("namespace", Namespace),
                new XAttribute("name", Type),
                // TODO: Old code added "full-name" as well, but that's redundant. Do we want to emulate it?
                // TODO: Why do we have a warnings element for warnings, but no types element for types? Inconsistent.
                new XElement("warnings", Warnings.Select(warning => new XElement("warning", warning))),
                Results.Select(x => x.ToXElement())
            );
        }

        public sealed class Builder
        {
            internal List<BenchmarkResult.Builder> Results { get; set; } = new List<BenchmarkResult.Builder>();

            internal List<string> Warnings { get; set; } = new List<string>();
            public string Namespace { get; set; }
            public string Type { get; set; }

            public BenchmarkTypeResults Build(BenchmarkRun run)
            {
                return new BenchmarkTypeResults(this, run);
            }

            public static Builder FromXElement(XElement element)
            {
                if (element == null)
                {
                    return null;
                }
                return new Builder
                {
                    Warnings = element.Elements("warnings").Elements("warning").Select(x => x.Value).ToList(),
                    Namespace = (string) element.Attribute("namespace"),
                    Type = (string) element.Attribute("name"),
                    Results = element.Elements(BenchmarkResult.ElementName).Select(BenchmarkResult.Builder.FromXElement).ToList()
                };
            }
        }
    }
}
