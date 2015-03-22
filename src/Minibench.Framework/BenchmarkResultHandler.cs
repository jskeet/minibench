// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Reflection;

namespace Minibench.Framework
{
    /// <summary>
    /// Handler for benchmark results.
    /// </summary>
    /// <remarks>
    /// While we could have used events on BenchmarkRunner, it's likely that a bunch of these will be
    /// customized at the same time, so it makes more sense to make it a normal class.
    /// </remarks>
    public abstract class BenchmarkResultHandler
    {
        /// <summary>
        /// Called at the very start of the set of tests.
        /// </summary>
        /// <param name="assembly">Assembly being benchmarked</param>
        /// <param name="environment">The environment in which the benchmark is being conducted</param>
        /// <param name="options">Options used in this test</param>
        public virtual void HandleStartRun(Assembly assembly, BenchmarkEnvironment environment, BenchmarkOptions options)
        {
        }

        /// <summary>
        /// Called at the very end of the set of tests.
        /// </summary>
        public virtual void HandleEndRun()
        {
        }

        /// <summary>
        /// Called at the start of benchmarks for a single type
        /// </summary>
        public virtual void HandleStartType(Type type)
        {
        }

        /// <summary>
        /// Called at the end of benchmarks for a single type.
        /// </summary>
        public virtual void HandleEndType()
        {
        }

        /// <summary>
        /// Called once for each test.
        /// </summary>
        /// <param name="result"></param>
        public virtual void HandleResult(string method, int iterations, TimeSpan duration)
        {
        }

        /// <summary>
        /// Called each time a type or method isn't tested unexpectedly.
        /// </summary>
        /// <param name="text"></param>
        public virtual void HandleWarning(string text)
        {
        }
    }
}
