// Copyright 2015 The Minibench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minibench.Framework
{
    /// <summary>
    /// Implementation of <see cref="BenchmarkResultHandler"/> which combines multiple handlers into one.
    /// Each handler call is simply proxied to all the original handlers, in the original order.
    /// </summary>
    public sealed class CompositeResultHandler : BenchmarkResultHandler
    {
        private readonly IList<BenchmarkResultHandler> handlers;

        public CompositeResultHandler(IEnumerable<BenchmarkResultHandler> handlers)
        {
            this.handlers = handlers.ToList();
        }

        public override void HandleStartRun(Assembly assembly, BenchmarkEnvironment environment, BenchmarkOptions options)
        {
            foreach (var handler in handlers)
            {
                handler.HandleStartRun(assembly, environment, options);
            }
        }

        public override void HandleStartType(Type type)
        {
            foreach (var handler in handlers)
            {
                handler.HandleStartType(type);
            }
        }

        public override void HandleResult(string method, int iterations, TimeSpan duration)
        {
            foreach (var handler in handlers)
            {
                handler.HandleResult(method, iterations, duration);
            }
        }

        public override void HandleEndType()
        {
            foreach (var handler in handlers)
            {
                handler.HandleEndType();
            }
        }

        public override void HandleWarning(string text)
        {
            foreach (var handler in handlers)
            {
                handler.HandleWarning(text);
            }            
        }

        public override void HandleEndRun()
        {
            foreach (var handler in handlers)
            {
                handler.HandleEndRun();
            }
        }
    }
}
