// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Xml.Linq;

namespace Minibench.Framework
{
    public sealed class BenchmarkEnvironment
    {
        public static readonly XName ElementName = "environment";

        public Version RuntimeVersion { get; }
        public string OperatingSystem { get; }
        public int Cores { get; }
        public bool Is64BitProcess { get; }
        public bool Is64BitOperatingSystem { get; }
        public string Machine { get; }

        public static BenchmarkEnvironment CurrentSystem { get; } = new Builder().Build();

        private BenchmarkEnvironment(Builder builder)
        {
            RuntimeVersion = builder.RuntimeVersion;
            OperatingSystem = builder.OperatingSystem;
            Cores = builder.Cores;
            Is64BitProcess = builder.Is64BitProcess;
            Is64BitOperatingSystem = builder.Is64BitOperatingSystem;
            Machine = builder.Machine;
        }

        public XElement ToXElement()
        {
            return new XElement(ElementName,
                new XAttribute("runtime", RuntimeVersion),
                new XAttribute("os", OperatingSystem),
                new XAttribute("cores", Cores),
                new XAttribute("is-64bit-process", Is64BitProcess),
                new XAttribute("is-64bit-os", Is64BitOperatingSystem),
                new XAttribute("machine", Machine));
        }

        public static BenchmarkEnvironment FromXElement(XElement element)
        {
            if (element == null)
            {
                return null;
            }
            return new Builder
            {
                RuntimeVersion = Version.Parse((string) element.Attribute("runtime")),
                OperatingSystem = (string) element.Attribute("os"),
                Cores = (int) element.Attribute("cores"),
                Is64BitProcess = (bool) element.Attribute("is-64bit-process"),
                Is64BitOperatingSystem = (bool) element.Attribute("is-64bit-os"),
                Machine = (string) element.Attribute("machine")
            }.Build();
        }

        public class Builder
        {
            public Version RuntimeVersion { get; set; }
            public string OperatingSystem { get; set; }
            public int Cores { get; set; }
            public bool Is64BitProcess { get; set; }
            public bool Is64BitOperatingSystem { get; set; }
            public string Machine { get; set; }

            public static Builder FromSystem()
            {
                return new Builder
                {
                    RuntimeVersion = Environment.Version,
                    OperatingSystem = Environment.OSVersion.VersionString,
                    Cores = Environment.ProcessorCount,
                    Is64BitProcess = Environment.Is64BitProcess,
                    Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                    Machine = Environment.MachineName
                };
            }

            public BenchmarkEnvironment Build()
            {
                return new BenchmarkEnvironment(this);
            }
        }
    }
}
