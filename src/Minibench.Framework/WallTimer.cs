// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Diagnostics;

namespace Minibench.Framework
{
    /// <summary>
    /// Timer using the built-in stopwatch class; measures wall-clock time.
    /// </summary>
    public sealed class WallTimer : IBenchTimer
    {
        private readonly Stopwatch stopwatch = Stopwatch.StartNew();

        public void Reset()
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

        public TimeSpan ElapsedTime => stopwatch.Elapsed;
    }
}