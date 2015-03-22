// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace Minibench.Framework
{
    /// <summary>
    /// Attribute applied to any method which should be benchmarked.
    /// The method must be parameterless, and its containing class
    /// must have a parameterless constructor. The constructor will
    /// be called just once, before all the tests are run - typically
    /// any initialization will just be for readonly fields.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BenchmarkAttribute : Attribute
    {
    }
}