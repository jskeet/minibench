// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using Minibench.Framework;
using System;

namespace Minibench.Demo
{
    public class MathBenchmarks
    {
        [Benchmark]
        public void Sqrt()
        {
            Math.Sqrt(2.0).Consume();
        }

        [Benchmark]
        public void Abs_Int32()
        {
            Math.Abs(-5).Consume();
        }
    }
}
