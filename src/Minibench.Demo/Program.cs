// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.
using Minibench.Framework;

namespace Minibench.Demo
{
    /// <summary>
    /// Small example to show how Minibench can be used.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.RunFromCommandLine(typeof(Program).Assembly);
        }
    }
}
