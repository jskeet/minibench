// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace Minibench.Framework
{
    /// <summary>
    /// Extension methods to make it easier to write benchmarks.
    /// </summary>
    public static class BenchmarkingExtensions
    {
        /// <summary>
        /// This does absolutely nothing, but 
        /// allows us to consume a property value without having
        /// a useless local variable. It should end up being JITted
        /// away completely, so have no effect on the results.
        /// </summary>
        public static void Consume<T>(this T value)
        {
        }
    }
}