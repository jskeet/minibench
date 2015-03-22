// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace Minibench.Framework
{
    /// <summary>
    /// Timer used to measure performance. Implementations may use wall time, CPU timing etc.
    /// Implementations don't need to be thread-safe.
    /// </summary>
    public interface IBenchTimer
    {
        void Reset();
        TimeSpan ElapsedTime { get; }
    }
}