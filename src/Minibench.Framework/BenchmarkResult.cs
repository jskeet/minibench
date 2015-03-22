// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Xml.Linq;

namespace Minibench.Framework
{
    /// <summary>
    /// The results of running a single test.
    /// </summary>
    public sealed class BenchmarkResult
    {
        /// <summary>
        /// XML element name for benchmark results.
        /// </summary>
        public static readonly XName ElementName = "result";

        internal const long TicksPerNanosecond = 100;
        internal const long TicksPerPicosecond = TicksPerNanosecond * 1000;

        /// <summary>
        /// Set of results for a single type that this result belongs to.
        /// </summary>
        public BenchmarkTypeResults TypeResults { get; }

        /// <summary>
        /// Convenience property to access the <see cref="BenchmarkRun" /> containing this result.
        /// </summary>
        public BenchmarkRun Run => TypeResults.Run;

        /// <summary>
        /// Name of the method tested.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Number of iterations executed.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Time taken to execute the test.
        /// </summary>
        public TimeSpan Duration { get; }

        private BenchmarkResult(Builder builder, BenchmarkTypeResults typeResults)
        {
            TypeResults = typeResults;
            Method = builder.Method;
            Iterations = builder.Iterations;
            Duration = builder.Duration;
        }

        /// <summary>
        /// The method name including namespace and type.
        /// </summary>
        public string FullMethod => $"{TypeResults.FullType}.{Method}";

        /// <summary>
        /// Number of calls per second (projected from the actual results).
        /// </summary>
        public long CallsPerSecond => Iterations * TimeSpan.TicksPerSecond / Duration.Ticks;

        /// <summary>
        /// Number of nanoseconds per call (projected from the actual results).
        /// </summary>
        public long NanosecondsPerCall => Duration.Ticks * TicksPerNanosecond / Iterations;

        /// <summary>
        /// Number of picoseconds per call (projected from the actual results).
        /// </summary>
        public long PicosecondsPerCall => Duration.Ticks * TicksPerPicosecond / Iterations;

        /// <summary>
        /// Creates an XML representation of this result.
        /// </summary>
        public XElement ToXElement()
        {
            return new XElement(ElementName,
                new XAttribute("method", Method),
                new XAttribute("iterations", Iterations),
                new XAttribute("duration", Duration.Ticks)
            );
        }

        /// <summary>
        /// Mutable builder type for results.
        /// </summary>
        public sealed class Builder
        {
            public string Method { get; set; }
            public int Iterations { get; set; }
            public TimeSpan Duration { get; set; }

            public long CallsPerSecond => Iterations * TimeSpan.TicksPerSecond / Duration.Ticks;

            public long NanosecondsPerCall => Duration.Ticks * TicksPerNanosecond / Iterations;

            public BenchmarkResult Build(BenchmarkTypeResults typeResults)
            {
                return new BenchmarkResult(this, typeResults);
            }

            public static Builder FromXElement(XElement element)
            {
                if (element == null)
                {
                    return null;
                }
                return new Builder
                {
                    Method = (string) element.Attribute("method"),
                    Iterations = (int) element.Attribute("iterations"),
                    Duration = TimeSpan.FromTicks((long) element.Attribute("duration"))
                };
            }
        }
    }
}